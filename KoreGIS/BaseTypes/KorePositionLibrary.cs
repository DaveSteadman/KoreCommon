// <fileheader>

using System.Collections.Generic;

using KoreCommon;

namespace KoreGIS;

// Usage: 
// - KoreLLPoint  myPoint = KorePositionLibrary.Positions["London"];
// - KoreLLPoint  myPoint = KorePositionLibrary.GetLLPos("London");
// - KoreLLAPoint myPoint = KorePositionLibrary.GetLLAPos("London");

public static class KorePositionLibrary
{
    public static readonly Dictionary<string, KoreLLPoint> Positions = new Dictionary<string, KoreLLPoint>
    {
        // ----------------------------------------------------------------------------------------
        // MARK: Europe 
        // ----------------------------------------------------------------------------------------

        // United Kingdom
        { "London",          new KoreLLPoint() {LatDegs = 51.5074, LonDegs = -0.1278} }, 
        { "Cardiff",         new KoreLLPoint() {LatDegs = 51.4816, LonDegs = -3.1791} }, 
        { "Edinburgh",       new KoreLLPoint() {LatDegs = 55.9533, LonDegs = -3.1883} }, 
        { "Belfast",         new KoreLLPoint() {LatDegs = 54.5973, LonDegs = -5.9301} }, 
        { "Liverpool",       new KoreLLPoint() {LatDegs = 53.4084, LonDegs = -2.9916} }, 
        { "Oxford",          new KoreLLPoint() {LatDegs = 51.7548, LonDegs = -1.2540} }, 
        { "Cambridge",       new KoreLLPoint() {LatDegs = 52.2053, LonDegs = 0.1218} }, 
        { "Manchester",      new KoreLLPoint() {LatDegs = 53.4808, LonDegs = -2.2426} }, 
        { "Birmingham",      new KoreLLPoint() {LatDegs = 52.4862, LonDegs = -1.8904} }, 
        { "Leeds",           new KoreLLPoint() {LatDegs = 53.8008, LonDegs = -1.5491} }, 
        { "Sheffield",       new KoreLLPoint() {LatDegs = 53.3811, LonDegs = -1.4701} }, 
        { "Glasgow",         new KoreLLPoint() {LatDegs = 55.8642, LonDegs = -4.2518} },
        { "Portsmouth",      new KoreLLPoint() {LatDegs = 50.8198, LonDegs = -1.0880} },
        { "Plymouth",        new KoreLLPoint() {LatDegs = 50.3755, LonDegs = -4.1427} },
        { "Norwich",         new KoreLLPoint() {LatDegs = 52.6309, LonDegs = 1.2973} },
        { "Bristol",         new KoreLLPoint() {LatDegs = 51.4545, LonDegs = -2.5879} },
        { "Southampton",     new KoreLLPoint() {LatDegs = 50.9097, LonDegs = -1.4044} },
        { "Leicester",       new KoreLLPoint() {LatDegs = 52.6369, LonDegs = -1.1398} },
        { "Coventry",        new KoreLLPoint() {LatDegs = 52.4084, LonDegs = -1.5100} },
        { "York",            new KoreLLPoint() {LatDegs = 53.9590, LonDegs = -1.0815} },
        { "Aberdeen",        new KoreLLPoint() {LatDegs = 57.1497, LonDegs = -2.0943} },

        // Ireland
        { "Dublin",          new KoreLLPoint() {LatDegs = 53.3498, LonDegs = -6.2603} }, 
        { "Cork",            new KoreLLPoint() {LatDegs = 51.8985, LonDegs = -8.4756} }, 
        { "Galway",          new KoreLLPoint() {LatDegs = 53.2707, LonDegs = -9.0568} }, 
        { "Limerick",        new KoreLLPoint() {LatDegs = 52.6634, LonDegs = -8.6267} },

        // France
        { "Paris",           new KoreLLPoint() {LatDegs = 48.8566, LonDegs = 2.3522} },
        { "Montpellier",     new KoreLLPoint() {LatDegs = 43.6117, LonDegs = 3.8767} },
        { "Marseille",       new KoreLLPoint() {LatDegs = 43.2965, LonDegs = 5.3698} },
        { "Nice",            new KoreLLPoint() {LatDegs = 43.7102, LonDegs = 7.2620} },
        { "Bordeaux",        new KoreLLPoint() {LatDegs = 44.8378, LonDegs = -0.5792} },
        { "Lyon",            new KoreLLPoint() {LatDegs = 45.7640, LonDegs = 4.8357} },
        { "Toulouse",        new KoreLLPoint() {LatDegs = 43.6047, LonDegs = 1.4442} },
        { "Nantes",          new KoreLLPoint() {LatDegs = 47.2184, LonDegs = -1.5536} },
        { "Strasbourg",      new KoreLLPoint() {LatDegs = 48.5734, LonDegs = 7.7521} },
        { "Lille",           new KoreLLPoint() {LatDegs = 50.6292, LonDegs = 3.0573} },
        { "Cannes",          new KoreLLPoint() {LatDegs = 43.5511, LonDegs = 7.0128} },
        { "LeMans",          new KoreLLPoint() {LatDegs = 47.9796, LonDegs = 0.2021} },

        // Germany
        { "Berlin",          new KoreLLPoint() {LatDegs = 52.5200, LonDegs = 13.4050} }, 
        { "Hamburg",         new KoreLLPoint() {LatDegs = 53.5511, LonDegs = 9.9937} },   
        { "Munich",          new KoreLLPoint() {LatDegs = 48.1351, LonDegs = 11.5820} },  
        { "Cologne",         new KoreLLPoint() {LatDegs = 50.9375, LonDegs = 6.9603} },   
        { "Frankfurt",       new KoreLLPoint() {LatDegs = 50.1109, LonDegs = 8.6821} },   
        { "Dusseldorf",      new KoreLLPoint() {LatDegs = 51.2277, LonDegs = 6.7735} },   
        { "Stuttgart",       new KoreLLPoint() {LatDegs = 48.7758, LonDegs = 9.1829} },   
        { "Leipzig",         new KoreLLPoint() {LatDegs = 51.3397, LonDegs = 12.3731} },  
        { "Bremen",          new KoreLLPoint() {LatDegs = 53.0793, LonDegs = 8.8017} },   
        { "Dresden",         new KoreLLPoint() {LatDegs = 51.0504, LonDegs = 13.7373} },  
        { "Hannover",        new KoreLLPoint() {LatDegs = 52.3759, LonDegs = 9.7320} },   
        { "Nuremberg",       new KoreLLPoint() {LatDegs = 49.4521, LonDegs = 11.0767} },          

        // Spain
        { "Madrid",          new KoreLLPoint() {LatDegs = 40.4168, LonDegs = -3.7038} }, 
        { "Barcelona",       new KoreLLPoint() {LatDegs = 41.3851, LonDegs = 2.1734} },
        { "Valencia",        new KoreLLPoint() {LatDegs = 39.4699, LonDegs = -0.3763} },
        { "Seville",         new KoreLLPoint() {LatDegs = 37.3891, LonDegs = -5.9845} },
        { "Bilbao",          new KoreLLPoint() {LatDegs = 43.2627, LonDegs = -2.9253} },
        { "Palma",           new KoreLLPoint() {LatDegs = 39.5696, LonDegs = 2.6502} },
        { "Malaga",          new KoreLLPoint() {LatDegs = 36.7213, LonDegs = -4.4214} },
        { "Granada",         new KoreLLPoint() {LatDegs = 37.1773, LonDegs = -3.5986} },

        // Portugal
        { "Lisbon",          new KoreLLPoint() {LatDegs = 38.7223, LonDegs = -9.1393} },  
        { "Porto",           new KoreLLPoint() {LatDegs = 41.1579, LonDegs = -8.6291} },  

        // Italy
        { "Rome",            new KoreLLPoint() {LatDegs = 41.9028, LonDegs = 12.4964} }, 
        { "Milan",           new KoreLLPoint() {LatDegs = 45.4642, LonDegs = 9.1900} },
        { "Palermo",         new KoreLLPoint() {LatDegs = 38.1157, LonDegs = 13.3615} },
        { "Naples",          new KoreLLPoint() {LatDegs = 40.8518, LonDegs = 14.2681} },
        { "Turin",           new KoreLLPoint() {LatDegs = 45.0703, LonDegs = 7.6869} },
        { "Bologna",         new KoreLLPoint() {LatDegs = 44.4949, LonDegs = 11.3426} },
        { "Florence",        new KoreLLPoint() {LatDegs = 43.7696, LonDegs = 11.2558} },
        { "Catania",         new KoreLLPoint() {LatDegs = 37.5079, LonDegs = 15.0830} },
        { "Genoa",           new KoreLLPoint() {LatDegs = 44.4056, LonDegs = 8.9463} },
        { "Venice",          new KoreLLPoint() {LatDegs = 45.4408, LonDegs = 12.3155} },

        // Switzerland
        { "Zurich",          new KoreLLPoint() {LatDegs = 47.3769, LonDegs = 8.5417} }, 
        { "Geneva",          new KoreLLPoint() {LatDegs = 46.2044, LonDegs = 6.1432} }, 
        { "Basel",           new KoreLLPoint() {LatDegs = 47.5596, LonDegs = 7.5886} }, 
        { "Bern",            new KoreLLPoint() {LatDegs = 46.9480, LonDegs = 7.4474} },

        // Sweden
        { "Stockholm",       new KoreLLPoint() {LatDegs = 59.3293, LonDegs = 18.0686} },
        { "Gothenburg",      new KoreLLPoint() {LatDegs = 57.7089, LonDegs = 11.9746} }, 
        { "Malmo",           new KoreLLPoint() {LatDegs = 55.6050, LonDegs = 13.0000} }, 

        // Norway
        { "Oslo",            new KoreLLPoint() {LatDegs = 59.9139, LonDegs = 10.7522} }, 
        { "Bergen",          new KoreLLPoint() {LatDegs = 60.3929, LonDegs = 5.3242} },

        // Denmark
        { "Copenhagen",      new KoreLLPoint() {LatDegs = 55.6761, LonDegs = 12.5683} }, 
        
        // Iceland
        { "Reykjavik",       new KoreLLPoint() {LatDegs = 64.1355, LonDegs = -21.8954} },

        // Finland
        { "Helsinki",        new KoreLLPoint() {LatDegs = 60.1699, LonDegs = 24.9384} },
        { "Turku",           new KoreLLPoint() {LatDegs = 60.4518, LonDegs = 22.2666} },
        { "Oulu",            new KoreLLPoint() {LatDegs = 65.0121, LonDegs = 25.4651} },

        // Greece
        { "Athens",          new KoreLLPoint() {LatDegs = 37.9838, LonDegs = 23.7275} }, 
        { "Marathon",        new KoreLLPoint() {LatDegs = 38.0833, LonDegs = 23.9667} },

        // Netherlands
        { "Amsterdam",       new KoreLLPoint() {LatDegs = 52.3676, LonDegs = 4.9041} },
        { "Rotterdam",       new KoreLLPoint() {LatDegs = 51.9225, LonDegs = 4.4792} }, 
        { "TheHague",        new KoreLLPoint() {LatDegs = 52.0705, LonDegs = 4.3007} },  

        // Belgium
        { "Brussels",        new KoreLLPoint() {LatDegs = 50.8503, LonDegs = 4.3517} },
        { "Antwerp",         new KoreLLPoint() {LatDegs = 51.2211, LonDegs = 4.4213} }, 
        { "Ghent",           new KoreLLPoint() {LatDegs = 51.0543, LonDegs = 3.7174} },

        // Austria
        { "Vienna",          new KoreLLPoint() {LatDegs = 48.2082, LonDegs = 16.3738} },
        { "Salzburg",        new KoreLLPoint() {LatDegs = 47.8095, LonDegs = 13.0550} }, 
        { "Innsbruck",       new KoreLLPoint() {LatDegs = 47.2692, LonDegs = 11.4041} }, 

        // Poland
        { "Warsaw",          new KoreLLPoint() {LatDegs = 52.2297, LonDegs = 21.0122} },
        { "Gdansk",          new KoreLLPoint() {LatDegs = 54.3520, LonDegs = 18.6466} },
        { "Krakow",          new KoreLLPoint() {LatDegs = 50.0647, LonDegs = 19.9450} },

        // Czech Republic
        { "Prague",          new KoreLLPoint() {LatDegs = 50.0755, LonDegs = 14.4378} },

        // Hungary
        { "Budapest",        new KoreLLPoint() {LatDegs = 47.4979, LonDegs = 19.0402} },  

        // Romania
        { "Bucharest",       new KoreLLPoint() {LatDegs = 44.4268, LonDegs = 26.1025} },

        // Bulgaria
        { "Sofia",           new KoreLLPoint() {LatDegs = 42.6977, LonDegs = 23.3219} },
        { "Varna",           new KoreLLPoint() {LatDegs = 43.2141, LonDegs = 27.9147} }, 

        // Croatia
        { "Zagreb",          new KoreLLPoint() {LatDegs = 45.8150, LonDegs = 15.9819} },
        { "Split",           new KoreLLPoint() {LatDegs = 43.5081, LonDegs = 16.4402} }, 
        { "Dubrovnik",       new KoreLLPoint() {LatDegs = 42.6507, LonDegs = 18.0944} },

        // Slovenia
        { "Ljubljana",       new KoreLLPoint() {LatDegs = 46.0569, LonDegs = 14.5058} },

        // Slovakia
        { "Bratislava",      new KoreLLPoint() {LatDegs = 48.1486, LonDegs = 17.1077} },

        // Albania
        { "Tirana",          new KoreLLPoint() {LatDegs = 41.3275, LonDegs = 19.8189} },

        // Kosovo
        { "Pristina",        new KoreLLPoint() {LatDegs = 42.6629, LonDegs = 21.1655} },
        
        // Baltic States
        { "Tallinn",         new KoreLLPoint() {LatDegs = 59.4370, LonDegs = 24.7536} },  // Estonia
        { "Riga",            new KoreLLPoint() {LatDegs = 56.9496, LonDegs = 24.1052} },  // Latvia
        { "Vilnius",         new KoreLLPoint() {LatDegs = 54.6872, LonDegs = 25.2797} },  // Lithuania

        // Ukraine
        { "Kyiv",            new KoreLLPoint() {LatDegs = 50.4501, LonDegs = 30.5234} },
        { "Odesa",           new KoreLLPoint() {LatDegs = 46.4825, LonDegs = 30.7233} },
        { "Lviv",            new KoreLLPoint() {LatDegs = 49.8397, LonDegs = 24.0297} },
        { "Kharkiv",         new KoreLLPoint() {LatDegs = 49.9935, LonDegs = 36.2304} },
        { "Dnipro",          new KoreLLPoint() {LatDegs = 48.4647, LonDegs = 35.0462} },

        // Russia
        { "Moscow",          new KoreLLPoint() {LatDegs = 55.7558, LonDegs = 37.6176} },
        { "SaintPetersburg", new KoreLLPoint() {LatDegs = 59.9343, LonDegs = 30.3351} },
        { "Sochi",           new KoreLLPoint() {LatDegs = 43.5853, LonDegs = 39.7203} },
        { "Vladivostok",     new KoreLLPoint() {LatDegs = 43.1156, LonDegs = 131.8855} },

        // Belarus
        { "Minsk",           new KoreLLPoint() {LatDegs = 53.9045, LonDegs = 27.5590} },

        // Microstates
        { "Monaco",          new KoreLLPoint() {LatDegs = 43.7384, LonDegs = 7.4246} },
        { "SanMarino",       new KoreLLPoint() {LatDegs = 43.9333, LonDegs = 12.4667} },
        { "VaticanCity",     new KoreLLPoint() {LatDegs = 41.9029, LonDegs = 12.4534} },
        { "Andorra",         new KoreLLPoint() {LatDegs = 42.5063, LonDegs = 1.5211} },
        { "Luxembourg",      new KoreLLPoint() {LatDegs = 49.6118, LonDegs = 6.1319} },
        { "Liechtenstein",   new KoreLLPoint() {LatDegs = 47.1415, LonDegs = 9.5215} },
        { "Gibraltar",       new KoreLLPoint() {LatDegs = 36.1408, LonDegs = -5.3536} },

        // ----------------------------------------------------------------------------------------
        // MARK: Asia 
        // ----------------------------------------------------------------------------------------

        // Saudi
        { "Riyadh",          new KoreLLPoint() {LatDegs = 24.7136, LonDegs = 46.6753} },
        { "Jeddah",          new KoreLLPoint() {LatDegs = 21.4858, LonDegs = 39.1925} },

        // UAE
        { "AbuDhabi",        new KoreLLPoint() {LatDegs = 24.4539, LonDegs = 54.3773} },
        { "Dubai",           new KoreLLPoint() {LatDegs = 25.2048, LonDegs = 55.2708} },
        { "AlAin",           new KoreLLPoint() {LatDegs = 24.1992, LonDegs = 55.7602} },

        // Middle East
        { "Tehran",          new KoreLLPoint() {LatDegs = 35.6892, LonDegs = 51.3890} },
        { "Baghdad",         new KoreLLPoint() {LatDegs = 33.3152, LonDegs = 44.3661} },
        { "Damascus",        new KoreLLPoint() {LatDegs = 33.5138, LonDegs = 36.2765} },
        { "Beirut",          new KoreLLPoint() {LatDegs = 33.8938, LonDegs = 35.5018} },
        { "Amman",           new KoreLLPoint() {LatDegs = 31.9454, LonDegs = 35.9284} },
        { "Jerusalem",       new KoreLLPoint() {LatDegs = 31.7683, LonDegs = 35.2137} },
        { "TelAviv",         new KoreLLPoint() {LatDegs = 32.0853, LonDegs = 34.7818} },
        { "Kuwait",          new KoreLLPoint() {LatDegs = 29.3759, LonDegs = 47.9774} },
        { "Doha",            new KoreLLPoint() {LatDegs = 25.2854, LonDegs = 51.5310} }, // Qatar
        { "Muscat",          new KoreLLPoint() {LatDegs = 23.5859, LonDegs = 58.4059} }, // Oman
        { "Sanaa",           new KoreLLPoint() {LatDegs = 15.3694, LonDegs = 44.1910} },
        { "Baku",            new KoreLLPoint() {LatDegs = 40.4093, LonDegs = 49.8671} },
        { "Tbilisi",         new KoreLLPoint() {LatDegs = 41.7151, LonDegs = 44.8271} },
        { "Istanbul",        new KoreLLPoint() {LatDegs = 41.0082, LonDegs = 28.9784} },

        // Pakistan
        { "Karachi",         new KoreLLPoint() {LatDegs = 24.8607, LonDegs = 67.0011} },
        { "Hyderabad",       new KoreLLPoint() {LatDegs = 17.3850, LonDegs = 78.4867} },
        { "Lahore",          new KoreLLPoint() {LatDegs = 31.5497, LonDegs = 74.3436} },
        { "Islamabad",       new KoreLLPoint() {LatDegs = 33.6844, LonDegs = 73.0479} },

        // India
        { "NewDelhi",        new KoreLLPoint() {LatDegs = 28.6139, LonDegs = 77.2090} },
        { "Mumbai",          new KoreLLPoint() {LatDegs = 19.0760, LonDegs = 72.8777} },
        { "Bangalore",       new KoreLLPoint() {LatDegs = 12.9716, LonDegs = 77.5946} },
        { "Chennai",         new KoreLLPoint() {LatDegs = 13.0827, LonDegs = 80.2707} },
        { "Kolkata",         new KoreLLPoint() {LatDegs = 22.5726, LonDegs = 88.3639} },
        { "Jaipur",          new KoreLLPoint() {LatDegs = 26.9124, LonDegs = 75.7873} },

        // Southeast Asia
        { "Bangkok",         new KoreLLPoint() {LatDegs = 13.7563, LonDegs = 100.5018} },
        { "Manila",          new KoreLLPoint() {LatDegs = 14.5995, LonDegs = 120.9842} },
        { "Jakarta",         new KoreLLPoint() {LatDegs = -6.2088, LonDegs = 106.8456} },
        { "KualaLumpur",     new KoreLLPoint() {LatDegs = 3.1390, LonDegs = 101.6869} },
        { "Singapore",       new KoreLLPoint() {LatDegs = 1.3521, LonDegs = 103.8198} },
        { "Hanoi",           new KoreLLPoint() {LatDegs = 21.0285, LonDegs = 105.8542} },
        { "HoChiMinhCity",   new KoreLLPoint() {LatDegs = 10.8231, LonDegs = 106.6297} },
        { "PhnomPenh",       new KoreLLPoint() {LatDegs = 11.5564, LonDegs = 104.9282} },
        { "Vientiane",       new KoreLLPoint() {LatDegs = 17.9757, LonDegs = 102.6331} },
        { "Yangon",          new KoreLLPoint() {LatDegs = 16.8661, LonDegs = 96.1951} },
        { "Pyongyang",       new KoreLLPoint() {LatDegs = 39.0392, LonDegs = 125.7625} },

        // South Korea
        { "Seoul",           new KoreLLPoint() {LatDegs = 37.5665, LonDegs = 126.9780} },
        { "Busan",           new KoreLLPoint() {LatDegs = 35.1796, LonDegs = 129.0756} },

        // South Asia
        { "Dhaka",           new KoreLLPoint() {LatDegs = 23.8103, LonDegs = 90.4125} },
        { "Colombo",         new KoreLLPoint() {LatDegs = 6.9271, LonDegs = 79.8612} }, // Sri Lanka
        { "Kathmandu",       new KoreLLPoint() {LatDegs = 27.7172, LonDegs = 85.3240} }, // Nepal
        { "Thimphu",         new KoreLLPoint() {LatDegs = 27.4728, LonDegs = 89.6390} }, // Afghanistan
        { "Male",            new KoreLLPoint() {LatDegs = 4.1755, LonDegs = 73.5093} },
        { "Kabul",           new KoreLLPoint() {LatDegs = 34.5553, LonDegs = 69.2075} },

        // Central Asia
        { "Tashkent",        new KoreLLPoint() {LatDegs = 41.2995, LonDegs = 69.2401} },
        { "Almaty",          new KoreLLPoint() {LatDegs = 43.2381, LonDegs = 76.9452} },
        { "Bishkek",         new KoreLLPoint() {LatDegs = 42.8746, LonDegs = 74.5698} },
        { "Dushanbe",        new KoreLLPoint() {LatDegs = 38.5598, LonDegs = 68.7870} },
        { "Ashgabat",        new KoreLLPoint() {LatDegs = 37.9601, LonDegs = 58.3261} },

        // China
        { "Beijing",         new KoreLLPoint() {LatDegs = 39.9042, LonDegs = 116.4074} },
        { "Shanghai",        new KoreLLPoint() {LatDegs = 31.2304, LonDegs = 121.4737} },
        { "Guangzhou",       new KoreLLPoint() {LatDegs = 23.1291, LonDegs = 113.2644} },
        { "Shenzhen",        new KoreLLPoint() {LatDegs = 22.5431, LonDegs = 114.0579} },

        // Taiwan
        { "Taipei",          new KoreLLPoint() {LatDegs = 25.0330, LonDegs = 121.5654} },

        // Japan
        { "Tokyo",           new KoreLLPoint() {LatDegs = 35.6762, LonDegs = 139.6503} },
        { "Osaka",           new KoreLLPoint() {LatDegs = 34.6937, LonDegs = 135.5023} },
        { "Kyoto",           new KoreLLPoint() {LatDegs = 35.0116, LonDegs = 135.7681} },
        { "Sapporo",         new KoreLLPoint() {LatDegs = 43.0642, LonDegs = 141.3469} },

        // ----------------------------------------------------------------------------------------
        // MARK: Africa 
        // ----------------------------------------------------------------------------------------

        // North Africa
        { "Cairo",           new KoreLLPoint() {LatDegs = 30.0444, LonDegs = 31.2357} },
        { "Alexandria",      new KoreLLPoint() {LatDegs = 31.2001, LonDegs = 29.9187} },
        { "Tunis",           new KoreLLPoint() {LatDegs = 36.8065, LonDegs = 10.1815} },
        { "Algiers",         new KoreLLPoint() {LatDegs = 36.7538, LonDegs = 3.0588} },
        { "Casablanca",      new KoreLLPoint() {LatDegs = 33.5731, LonDegs = -7.5898} },
        { "Rabat",           new KoreLLPoint() {LatDegs = 34.0209, LonDegs = -6.8416} },
        { "Marrakesh",       new KoreLLPoint() {LatDegs = 31.6295, LonDegs = -7.9811} },
        { "Tangier",         new KoreLLPoint() {LatDegs = 35.7673, LonDegs = -5.7981} },
        { "Tripoli",         new KoreLLPoint() {LatDegs = 32.8872, LonDegs = 13.1913} },

        // West Africa
        { "Lagos",           new KoreLLPoint() {LatDegs = 6.5244, LonDegs = 3.3792} },
        { "Abuja",           new KoreLLPoint() {LatDegs = 9.0765, LonDegs = 7.3986} },
        { "Accra",           new KoreLLPoint() {LatDegs = 5.6037, LonDegs = -0.1870} },
        { "Abidjan",         new KoreLLPoint() {LatDegs = 5.3600, LonDegs = -4.0083} },
        { "Dakar",           new KoreLLPoint() {LatDegs = 14.7167, LonDegs = -17.4677} },
        { "Bamako",          new KoreLLPoint() {LatDegs = 12.6392, LonDegs = -8.0029} },
        { "Ouagadougou",     new KoreLLPoint() {LatDegs = 12.3714, LonDegs = -1.5197} },

        // East Africa
        { "Nairobi",         new KoreLLPoint() {LatDegs = -1.2921, LonDegs = 36.8219} },
        { "AddisAbaba",      new KoreLLPoint() {LatDegs = 9.1450, LonDegs = 40.4897} },
        { "Khartoum",        new KoreLLPoint() {LatDegs = 15.5007, LonDegs = 32.5599} },
        { "Kampala",         new KoreLLPoint() {LatDegs = 0.3476, LonDegs = 32.5825} },
        { "Kigali",          new KoreLLPoint() {LatDegs = -1.9441, LonDegs = 30.0619} },
        { "DarEsSalaam",     new KoreLLPoint() {LatDegs = -6.7924, LonDegs = 39.2083} },
        { "Dodoma",          new KoreLLPoint() {LatDegs = -6.1630, LonDegs = 35.7516} },

        // Southern Africa
        { "CapeTown",        new KoreLLPoint() {LatDegs = -33.9249, LonDegs = 18.4241} },
        { "Johannesburg",    new KoreLLPoint() {LatDegs = -26.2041, LonDegs = 28.0473} },
        { "Pretoria",        new KoreLLPoint() {LatDegs = -25.7479, LonDegs = 28.2293} },
        { "Lusaka",          new KoreLLPoint() {LatDegs = -15.3875, LonDegs = 28.3228} },
        { "Harare",          new KoreLLPoint() {LatDegs = -17.8292, LonDegs = 31.0522} },
        { "Maputo",          new KoreLLPoint() {LatDegs = -25.9692, LonDegs = 32.5732} },
        { "Windhoek",        new KoreLLPoint() {LatDegs = -22.5609, LonDegs = 17.0658} },
        { "Gaborone",        new KoreLLPoint() {LatDegs = -24.6282, LonDegs = 25.9231} },
        { "Maseru",          new KoreLLPoint() {LatDegs = -29.3151, LonDegs = 27.4869} },
        { "Mbabane",         new KoreLLPoint() {LatDegs = -26.3054, LonDegs = 31.1367} },
        { "Durban",          new KoreLLPoint() {LatDegs = -29.8587, LonDegs = 31.0218} },

        // Indian Ocean Islands
        { "Antananarivo",    new KoreLLPoint() {LatDegs = -18.8792, LonDegs = 47.5079} },
        { "PortLouis",       new KoreLLPoint() {LatDegs = -20.1609, LonDegs = 57.5012} },

        // ----------------------------------------------------------------------------------------
        // MARK: North America
        // ----------------------------------------------------------------------------------------

        // United States
        { "NewYork",         new KoreLLPoint() {LatDegs = 40.7128, LonDegs = -74.0060} },
        { "Boston",          new KoreLLPoint() {LatDegs = 42.3601, LonDegs = -71.0589} },
        { "Philadelphia",    new KoreLLPoint() {LatDegs = 39.9526, LonDegs = -75.1652} },
        { "WashingtonDC",    new KoreLLPoint() {LatDegs = 38.9072, LonDegs = -77.0369} },
        { "Miami",           new KoreLLPoint() {LatDegs = 25.7617, LonDegs = -80.1918} },
        { "Atlanta",         new KoreLLPoint() {LatDegs = 33.7490, LonDegs = -84.3880} },
        { "Dallas",          new KoreLLPoint() {LatDegs = 32.7767, LonDegs = -96.7970} },
        { "Houston",         new KoreLLPoint() {LatDegs = 29.7604, LonDegs = -95.3698} },
        { "Detroit",         new KoreLLPoint() {LatDegs = 42.3314, LonDegs = -83.0458} },
        { "Chicago",         new KoreLLPoint() {LatDegs = 41.8781, LonDegs = -87.6298} },
        { "KansasCity",      new KoreLLPoint() {LatDegs = 39.0997, LonDegs = -94.5786} },
        { "Denver",          new KoreLLPoint() {LatDegs = 39.7392, LonDegs = -104.9903} },
        { "Albuquerque",     new KoreLLPoint() {LatDegs = 35.0844, LonDegs = -106.6504} },
        { "Phoenix",         new KoreLLPoint() {LatDegs = 33.4484, LonDegs = -112.0740} },
        { "LasVegas",        new KoreLLPoint() {LatDegs = 36.1699, LonDegs = -115.1398} },
        { "Seattle",         new KoreLLPoint() {LatDegs = 47.6062, LonDegs = -122.3321} },
        { "Portland",        new KoreLLPoint() {LatDegs = 45.5155, LonDegs = -122.6793} },
        { "SanFrancisco",    new KoreLLPoint() {LatDegs = 37.7749, LonDegs = -122.4194} },
        { "LosAngeles",      new KoreLLPoint() {LatDegs = 34.0522, LonDegs = -118.2437} },
        { "SanDiego",        new KoreLLPoint() {LatDegs = 32.7157, LonDegs = -117.1611} },
        { "Anchorage",       new KoreLLPoint() {LatDegs = 61.2181, LonDegs = -149.9003} },
        { "Honolulu",        new KoreLLPoint() {LatDegs = 21.3069, LonDegs = -157.8583} },

        // Canada
        { "Montreal",        new KoreLLPoint() {LatDegs = 45.5017, LonDegs = -73.5673} },
        { "Ottawa",          new KoreLLPoint() {LatDegs = 45.4215, LonDegs = -75.6972} },
        { "Toronto",         new KoreLLPoint() {LatDegs = 43.6511, LonDegs = -79.3470} },
        { "Calgary",         new KoreLLPoint() {LatDegs = 51.0447, LonDegs = -114.0719} },
        { "Edmonton",        new KoreLLPoint() {LatDegs = 53.5461, LonDegs = -113.4938} },
        { "Vancouver",       new KoreLLPoint() {LatDegs = 49.2827, LonDegs = -123.1207} },

        // Mexico
        { "MexicoCity",      new KoreLLPoint() {LatDegs = 19.4326, LonDegs = -99.1332} },
        { "Guadalajara",     new KoreLLPoint() {LatDegs = 20.6597, LonDegs = -103.3496} },
        { "Monterrey",       new KoreLLPoint() {LatDegs = 25.6866, LonDegs = -100.3161} },
        { "Tijuana",         new KoreLLPoint() {LatDegs = 32.5149, LonDegs = -117.0382} },

        // Caribbean & Central America
        { "Cancun",          new KoreLLPoint() {LatDegs = 21.1619, LonDegs = -86.8515} },
        { "Havana",          new KoreLLPoint() {LatDegs = 23.1136, LonDegs = -82.3666} },
        { "SantoDomingo",    new KoreLLPoint() {LatDegs = 18.4861, LonDegs = -69.9312} },
        { "SanJose",         new KoreLLPoint() {LatDegs = 9.9281, LonDegs = -84.0907} },
        { "Guatemala",       new KoreLLPoint() {LatDegs = 14.6349, LonDegs = -90.5069} },

        // ----------------------------------------------------------------------------------------
        // MARK: South America
        // ----------------------------------------------------------------------------------------

        // Brazil
        { "Brasilia",        new KoreLLPoint() {LatDegs = -15.8267, LonDegs = -47.9218} },
        { "SaoPaulo",        new KoreLLPoint() {LatDegs = -23.5505, LonDegs = -46.6333} },
        { "RioDeJaneiro",    new KoreLLPoint() {LatDegs = -22.9068, LonDegs = -43.1729} },
        { "Salvador",        new KoreLLPoint() {LatDegs = -12.9714, LonDegs = -38.5014} },
        
        // Argentina
        { "BuenosAires",     new KoreLLPoint() {LatDegs = -34.6118, LonDegs = -58.3960} },
        { "Cordoba",         new KoreLLPoint() {LatDegs = -31.4201, LonDegs = -64.1888} },

        // Other South American Countries
        { "Lima",            new KoreLLPoint() {LatDegs = -12.0464, LonDegs = -77.0428} },
        { "Bogota",          new KoreLLPoint() {LatDegs = 4.7110, LonDegs = -74.0721} },
        { "Caracas",         new KoreLLPoint() {LatDegs = 10.4806, LonDegs = -66.9036} },
        { "Santiago",        new KoreLLPoint() {LatDegs = -33.4489, LonDegs = -70.6693} },
        { "LaPaz",           new KoreLLPoint() {LatDegs = -16.5000, LonDegs = -68.1500} },
        { "Sucre",           new KoreLLPoint() {LatDegs = -19.0196, LonDegs = -65.2619} },
        { "Quito",           new KoreLLPoint() {LatDegs = -0.1807, LonDegs = -78.4678} },
        { "Asuncion",        new KoreLLPoint() {LatDegs = -25.2637, LonDegs = -57.5759} },
        { "Montevideo",      new KoreLLPoint() {LatDegs = -34.9011, LonDegs = -56.1645} },
        { "Georgetown",      new KoreLLPoint() {LatDegs = 6.8013, LonDegs = -58.1551} },
        { "Paramaribo",      new KoreLLPoint() {LatDegs = 5.8520, LonDegs = -55.2038} },
        { "Cayenne",         new KoreLLPoint() {LatDegs = 4.9331, LonDegs = -52.3267} },

        // ----------------------------------------------------------------------------------------
        // MARK: Australia, New Zealand, and Pacific Islands
        // ----------------------------------------------------------------------------------------

        // Australia
        { "Sydney",          new KoreLLPoint() {LatDegs = -33.8688, LonDegs = 151.2093} },
        { "Melbourne",       new KoreLLPoint() {LatDegs = -37.8136, LonDegs = 144.9631} },
        { "Brisbane",        new KoreLLPoint() {LatDegs = -27.4698, LonDegs = 153.0251} },
        { "Perth",           new KoreLLPoint() {LatDegs = -31.9505, LonDegs = 115.8605} },
        { "Adelaide",        new KoreLLPoint() {LatDegs = -34.9285, LonDegs = 138.6007} },
        { "Hobart",          new KoreLLPoint() {LatDegs = -42.8821, LonDegs = 147.3272} },
        { "Canberra",        new KoreLLPoint() {LatDegs = -35.2809, LonDegs = 149.1300} },
        { "Darwin",          new KoreLLPoint() {LatDegs = -12.4634, LonDegs = 130.8418} },

        // New Zealand
        { "Wellington",      new KoreLLPoint() {LatDegs = -41.2865, LonDegs = 174.7762} },
        { "Auckland",        new KoreLLPoint() {LatDegs = -36.8485, LonDegs = 174.7633} },
        { "Christchurch",    new KoreLLPoint() {LatDegs = -43.5321, LonDegs = 172.6362} },
        
        // Pacific Islands
        { "Suva",            new KoreLLPoint() {LatDegs = -18.1248, LonDegs = 178.4501} },
        { "PortMoresby",     new KoreLLPoint() {LatDegs = -9.4438, LonDegs = 147.1803} },
        { "NukuAlofa",       new KoreLLPoint() {LatDegs = -21.1789, LonDegs = -175.1982} },
        { "Apia",            new KoreLLPoint() {LatDegs = -13.8506, LonDegs = -171.7513} },
        { "PortVila",        new KoreLLPoint() {LatDegs = -17.7334, LonDegs = 168.3273} },
        { "Honiara",         new KoreLLPoint() {LatDegs = -9.4280, LonDegs = 159.9500} },
        { "Tarawa",          new KoreLLPoint() {LatDegs = 1.3278, LonDegs = 172.9783} },
        { "Majuro",          new KoreLLPoint() {LatDegs = 7.1315, LonDegs = 171.1845} },
    };

    // ----------------------------------------------------------------------------------------
    // MARK: Functions 
    // ----------------------------------------------------------------------------------------

    public static KoreLLPoint GetLLPos(string name)
    {
        if (Positions.TryGetValue(name, out var position))
            return position;

        return KoreLLPoint.Zero;
    }

    public static KoreLLAPoint GetLLAPos(string name)
    {
        if (Positions.TryGetValue(name, out var position))
            return new KoreLLAPoint(position);

        return KoreLLAPoint.Zero;
    }

}

