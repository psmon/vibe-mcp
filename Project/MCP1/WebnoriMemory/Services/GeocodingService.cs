using System.Text.RegularExpressions;

namespace WebnoriMemory.Services;

public class GeocodingService
{
    // Simple location database for Korean cities
    private readonly Dictionary<string, (double Latitude, double Longitude)> _locationDatabase = new()
    {
        // Major cities
        ["서울"] = (37.5665, 126.9780),
        ["부산"] = (35.1796, 129.0756),
        ["대구"] = (35.8714, 128.6014),
        ["인천"] = (37.4563, 126.7052),
        ["광주"] = (35.1595, 126.8526),
        ["대전"] = (36.3504, 127.3845),
        ["울산"] = (35.5384, 129.3114),
        ["세종"] = (36.4800, 127.2890),
        
        // Gyeongsangnam-do cities
        ["창원"] = (35.2280, 128.6811),
        ["진주"] = (35.1799, 128.1076),
        ["통영"] = (34.8544, 128.4330),
        ["사천"] = (35.0035, 128.0644),
        ["김해"] = (35.2285, 128.8894),
        ["밀양"] = (35.5037, 128.7489),
        ["거제"] = (34.8806, 128.6210),
        ["양산"] = (35.3350, 129.0370),
        ["의령"] = (35.3222, 128.2617),
        ["함안"] = (35.2728, 128.4065),
        ["창녕"] = (35.5447, 128.4922),
        ["고성"] = (34.9730, 128.3220),
        ["남해"] = (34.8377, 127.8925),
        ["하동"] = (35.0673, 127.7512),
        ["산청"] = (35.4156, 127.8738),
        ["함양"] = (35.5205, 127.7251),
        ["거창"] = (35.6867, 127.9095),
        ["합천"] = (35.5668, 128.1658),
        
        // Additional common locations
        ["제주"] = (33.4890, 126.4983),
        ["강릉"] = (37.7519, 128.8761),
        ["경주"] = (35.8562, 129.2247),
        ["전주"] = (35.8242, 127.1480),
        ["춘천"] = (37.8813, 127.7300),
        ["청주"] = (36.6424, 127.4890),
        ["천안"] = (36.8151, 127.1139),
        ["포항"] = (36.0190, 129.3435),
        ["원주"] = (37.3422, 127.9202),
        ["수원"] = (37.2636, 127.0286),
        ["성남"] = (37.4200, 127.1267),
        ["안양"] = (37.3943, 126.9568),
        ["부천"] = (37.5034, 126.7660),
        ["광명"] = (37.4786, 126.8644),
        ["평택"] = (36.9921, 127.1127),
        ["안산"] = (37.3219, 126.8309),
        ["고양"] = (37.6584, 126.8320),
        ["과천"] = (37.4291, 126.9876),
        ["구리"] = (37.5943, 127.1296),
        ["남양주"] = (37.6360, 127.2167),
        ["오산"] = (37.1498, 127.0772),
        ["시흥"] = (37.3801, 126.8031),
        ["군포"] = (37.3617, 126.9352),
        ["의왕"] = (37.3447, 126.9686),
        ["하남"] = (37.5393, 127.2147),
        ["용인"] = (37.2411, 127.1776),
        ["파주"] = (37.7599, 126.7799),
        ["이천"] = (37.2720, 127.4348),
        ["안성"] = (37.0080, 127.2704),
        ["김포"] = (37.6154, 126.7156),
        ["화성"] = (37.1998, 126.8310),
        ["광주시"] = (37.4295, 127.2554),
        ["양주"] = (37.7852, 127.0458),
        ["포천"] = (37.8950, 127.2003),
        ["여주"] = (37.2982, 127.6367),
        ["연천"] = (38.0963, 127.0750),
        ["가평"] = (37.8315, 127.5096),
        ["양평"] = (37.4917, 127.4876)
    };

    public (double? Latitude, double? Longitude, string? LocationName) ExtractLocationFromText(string text)
    {
        if (string.IsNullOrEmpty(text))
            return (null, null, null);

        // Normalize text
        var normalizedText = text.Replace(" ", "").ToLower();

        // Look for location patterns
        foreach (var location in _locationDatabase)
        {
            // Check for exact match or pattern like "창원에", "창원에서", "창원으로" etc.
            var patterns = new[]
            {
                location.Key,
                $"{location.Key}에",
                $"{location.Key}에서",
                $"{location.Key}으로",
                $"{location.Key}로",
                $"{location.Key}의",
                $"{location.Key}를",
                $"{location.Key}을",
                $"{location.Key}이",
                $"{location.Key}가",
                $"{location.Key}은",
                $"{location.Key}는",
                $"{location.Key}과",
                $"{location.Key}와",
                $"{location.Key}도"
            };

            foreach (var pattern in patterns)
            {
                if (normalizedText.Contains(pattern.ToLower()))
                {
                    return (location.Value.Latitude, location.Value.Longitude, location.Key);
                }
            }
        }

        // Try to find district names within Seoul
        var seoulDistricts = new Dictionary<string, string>
        {
            ["강남"] = "강남구",
            ["강동"] = "강동구",
            ["강북"] = "강북구",
            ["강서"] = "강서구",
            ["관악"] = "관악구",
            ["광진"] = "광진구",
            ["구로"] = "구로구",
            ["금천"] = "금천구",
            ["노원"] = "노원구",
            ["도봉"] = "도봉구",
            ["동대문"] = "동대문구",
            ["동작"] = "동작구",
            ["마포"] = "마포구",
            ["서대문"] = "서대문구",
            ["서초"] = "서초구",
            ["성동"] = "성동구",
            ["성북"] = "성북구",
            ["송파"] = "송파구",
            ["양천"] = "양천구",
            ["영등포"] = "영등포구",
            ["용산"] = "용산구",
            ["은평"] = "은평구",
            ["종로"] = "종로구",
            ["중구"] = "중구",
            ["중랑"] = "중랑구"
        };

        foreach (var district in seoulDistricts)
        {
            if (normalizedText.Contains(district.Key))
            {
                // Return Seoul coordinates with district name
                var seoul = _locationDatabase["서울"];
                return (seoul.Latitude, seoul.Longitude, $"서울 {district.Value}");
            }
        }

        return (null, null, null);
    }

    public double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth radius in kilometers
        
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        
        return R * c;
    }

    private double ToRadians(double degrees)
    {
        return degrees * (Math.PI / 180);
    }
}