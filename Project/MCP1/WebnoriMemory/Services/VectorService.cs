using System.Security.Cryptography;
using System.Text;

namespace WebnoriMemory.Services;

public class VectorService
{
    private const int VectorDimension = 128;

    // Dummy implementation that generates consistent vectors from text
    public float[] GenerateEmbedding(string text)
    {
        if (string.IsNullOrEmpty(text))
            return new float[VectorDimension];

        // Use SHA256 to generate consistent pseudo-random values from text
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(text));
        
        var vector = new float[VectorDimension];
        var random = new Random(BitConverter.ToInt32(hashBytes, 0));
        
        for (int i = 0; i < VectorDimension; i++)
        {
            vector[i] = (float)(random.NextDouble() * 2 - 1); // Range -1 to 1
        }
        
        // Normalize the vector
        var magnitude = (float)Math.Sqrt(vector.Sum(x => x * x));
        if (magnitude > 0)
        {
            for (int i = 0; i < vector.Length; i++)
            {
                vector[i] /= magnitude;
            }
        }
        
        return vector;
    }

    // Calculate cosine similarity between two vectors
    public float CalculateSimilarity(float[] vector1, float[] vector2)
    {
        if (vector1.Length != vector2.Length)
            throw new ArgumentException("Vectors must have the same dimension");

        float dotProduct = 0;
        for (int i = 0; i < vector1.Length; i++)
        {
            dotProduct += vector1[i] * vector2[i];
        }

        return dotProduct; // Vectors are already normalized
    }

    // Find similar titles based on vector similarity
    public List<(string Id, float Similarity)> FindSimilarTitles(string queryTitle, List<(string Id, string Title, float[] Embedding)> notes, int topK = 5)
    {
        var queryEmbedding = GenerateEmbedding(queryTitle);
        var similarities = new List<(string Id, float Similarity)>();

        foreach (var note in notes)
        {
            if (note.Embedding != null)
            {
                var similarity = CalculateSimilarity(queryEmbedding, note.Embedding);
                similarities.Add((note.Id, similarity));
            }
        }

        return similarities
            .OrderByDescending(x => x.Similarity)
            .Take(topK)
            .ToList();
    }
}