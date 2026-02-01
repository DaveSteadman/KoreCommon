// <fileheader>

using System;
using System.IO;
using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;
using SkiaSharp;

using KorePlotter.UnitTest;

namespace KoreCommon.Server;


// C# equivalent of Image-TestServer.ipynb
// Uses HttpListener to create an HTTP server that generates RGBA PNG images
// based on JSON POST requests (compatible with FastAPI client).

class ProgramServer
{
    private HttpListener? listener;
    private bool running;
    private Thread? serverThread;

    // --------------------------------------------------------------------------------------------
    // MARK: Constructor
    // --------------------------------------------------------------------------------------------

    public ProgramServer()
    {
        listener = null;
        running = false;
        serverThread = null;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Start/Stop Server
    // --------------------------------------------------------------------------------------------

    public void StartServer(string ipAddr = "127.0.0.1", int port = 8000)
    {
        Console.WriteLine($"Starting HTTP image server on {ipAddr}:{port}");

        listener = new HttpListener();
        listener.Prefixes.Add($"http://{ipAddr}:{port}/");
        listener.Start();
        running = true;

        Console.WriteLine($"Server started on http://{ipAddr}:{port}");
        Console.WriteLine("Server is ready to accept HTTP requests.");

        // Start processing in a background thread
        serverThread = new Thread(ProcessRequests);
        serverThread.Start();
    }

    public void StopServer()
    {
        Console.WriteLine("Stopping server...");
        running = false;
        listener?.Stop();
        serverThread?.Join(1000);
        Console.WriteLine("Server stopped.");
    }

    // --------------------------------------------------------------------------------------------
    // MARK: HTTP Request Processing Loop
    // --------------------------------------------------------------------------------------------

    private void ProcessRequests()
    {
        while (running && listener != null)
        {
            try
            {
                // Wait for a request (blocking call)
                HttpListenerContext context = listener.GetContext();

                // Handle the request asynchronously
                Task.Run(() => HandleHttpRequest(context));
            }
            catch (HttpListenerException)
            {
                // Expected when listener is stopped
                break;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error in request loop: {ex.Message}");
            }
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: HTTP Request Handler
    // --------------------------------------------------------------------------------------------

    private void HandleHttpRequest(HttpListenerContext context)
    {
        HttpListenerRequest request = context.Request;
        HttpListenerResponse response = context.Response;

        try
        {
            // Handle /health endpoint
            if (request.Url?.AbsolutePath == "/health" && request.HttpMethod == "GET")
            {
                SendJsonResponse(response, 200, new { status = "ok" });
                Console.WriteLine("Health check: OK");
                return;
            }

            // Handle /render endpoint
            if (request.Url?.AbsolutePath == "/render" && request.HttpMethod == "POST")
            {
                HandleRenderRequest(request, response);
                return;
            }

            // 404 for unknown endpoints
            SendJsonResponse(response, 404, new { error = "Not Found" });
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error handling request: {ex.Message}");
            SendJsonResponse(response, 500, new { error = ex.Message });
        }
    }

    private void HandleRenderRequest(HttpListenerRequest request, HttpListenerResponse response)
    {
        try
        {
            // Read and parse JSON body
            string body;
            using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
            {
                body = reader.ReadToEnd();
            }

            var imageRequest = JsonSerializer.Deserialize<ImageRequest>(body);
            if (imageRequest == null)
            {
                SendJsonResponse(response, 400, new { error = "Invalid request format" });
                return;
            }

            // Validate size
            if (imageRequest.size < 1 || imageRequest.size > 2048)
            {
                SendJsonResponse(response, 400, new { error = "Size must be between 1 and 2048" });
                return;
            }

            // Generate the image
            byte[] pngBytes = RenderRgbaPng(imageRequest.size, imageRequest.contents ?? "");

            // Create response
            var imageResponse = new ImageResponse
            {
                width = imageRequest.size,
                height = imageRequest.size,
                mime = "image/png",
                rgba_png_base64 = Convert.ToBase64String(pngBytes)
            };

            // Send response
            SendJsonResponse(response, 200, imageResponse);
            Console.WriteLine($"Processed request: size={imageRequest.size}, contents={imageRequest.contents}");
        }
        catch (Exception ex)
        {
            Console.WriteLine($"Error in render request: {ex.Message}");
            SendJsonResponse(response, 500, new { error = ex.Message });
        }
    }

    private void SendJsonResponse(HttpListenerResponse response, int statusCode, object data)
    {
        response.StatusCode = statusCode;
        response.ContentType = "application/json";

        string jsonResponse = JsonSerializer.Serialize(data);
        byte[] buffer = Encoding.UTF8.GetBytes(jsonResponse);

        response.ContentLength64 = buffer.Length;
        response.OutputStream.Write(buffer, 0, buffer.Length);
        response.OutputStream.Close();
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Image Generation
    // --------------------------------------------------------------------------------------------

    private byte[] RenderRgbaPng(int size, string contents)
    {
        // Create a new bitmap
        SKBitmap bitmap = new SKBitmap(size, size);
        using (SKCanvas canvas = new SKCanvas(bitmap))
        {
            canvas.Clear(SKColors.Transparent);

            string lowerContents = contents.Trim().ToLower();

            if (lowerContents.StartsWith("solid:"))
            {
                // Solid color: "solid:#112233" or "solid:red"
                string colorStr = contents.Substring(6).Trim();
                SKColor color = ParseColor(colorStr);
                canvas.Clear(color);
            }
            else if (lowerContents.StartsWith("checker:"))
            {
                // Checker pattern: "checker:black,white,8"
                RenderChecker(canvas, size, contents.Substring(8).Trim());
            }
            else if (lowerContents.StartsWith("text:"))
            {
                // Text: "text:hello"
                string text = contents.Substring(5);
                RenderText(canvas, size, text);



            }
            else
            {
                // Default: solid grey
                canvas.Clear(new SKColor(128, 128, 128, 255));
            }
        }

        // Encode to PNG
        using (var image = SKImage.FromBitmap(bitmap))
        using (var data = image.Encode(SKEncodedImageFormat.Png, 100))
        {
            return data.ToArray();
        }
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Color Parsing
    // --------------------------------------------------------------------------------------------

    private SKColor ParseColor(string name)
    {
        string s = name.Trim().ToLower();

        // Named colors
        var namedColors = new Dictionary<string, SKColor>
        {
            { "red", new SKColor(255, 0, 0, 255) },
            { "green", new SKColor(0, 255, 0, 255) },
            { "blue", new SKColor(0, 0, 255, 255) },
            { "white", new SKColor(255, 255, 255, 255) },
            { "black", new SKColor(0, 0, 0, 255) },
            { "transparent", new SKColor(0, 0, 0, 0) }
        };

        if (namedColors.ContainsKey(s))
        {
            return namedColors[s];
        }

        // Hex colors: #RRGGBB or #RRGGBBAA
        if (s.StartsWith("#"))
        {
            string hex = s.Substring(1);
            if (hex.Length == 6)
            {
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                return new SKColor(r, g, b, 255);
            }
            else if (hex.Length == 8)
            {
                byte r = Convert.ToByte(hex.Substring(0, 2), 16);
                byte g = Convert.ToByte(hex.Substring(2, 2), 16);
                byte b = Convert.ToByte(hex.Substring(4, 2), 16);
                byte a = Convert.ToByte(hex.Substring(6, 2), 16);
                return new SKColor(r, g, b, a);
            }
        }

        // RGBA format: "rgba(r,g,b,a)"
        var match = Regex.Match(s, @"rgba\(\s*(\d+)\s*,\s*(\d+)\s*,\s*(\d+)\s*,\s*([0-9.]+)\s*\)");
        if (match.Success)
        {
            byte r = ClampU8(int.Parse(match.Groups[1].Value));
            byte g = ClampU8(int.Parse(match.Groups[2].Value));
            byte b = ClampU8(int.Parse(match.Groups[3].Value));
            float aRaw = float.Parse(match.Groups[4].Value);
            byte a = aRaw <= 1.0f ? ClampU8((int)(aRaw * 255.0f)) : ClampU8((int)aRaw);
            return new SKColor(r, g, b, a);
        }

        // Fallback: mid grey
        return new SKColor(128, 128, 128, 255);
    }

    private byte ClampU8(int x)
    {
        if (x < 0) return 0;
        if (x > 255) return 255;
        return (byte)x;
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Rendering Patterns
    // --------------------------------------------------------------------------------------------

    private void RenderChecker(SKCanvas canvas, int size, string spec)
    {
        // Parse: "black,white,8" -> color1, color2, tiles
        string[] parts = spec.Split(',');
        SKColor c1 = parts.Length > 0 ? ParseColor(parts[0].Trim()) : SKColors.Black;
        SKColor c2 = parts.Length > 1 ? ParseColor(parts[1].Trim()) : SKColors.White;
        int n = parts.Length > 2 && int.TryParse(parts[2].Trim(), out int tiles) ? tiles : 8;
        n = Math.Max(1, Math.Min(256, n));
        int tileSize = Math.Max(1, size / n);

        using (var paint = new SKPaint())
        {
            for (int y = 0; y < size; y += tileSize)
            {
                for (int x = 0; x < size; x += tileSize)
                {
                    bool useFirst = ((x / tileSize) + (y / tileSize)) % 2 == 0;
                    paint.Color = useFirst ? c1 : c2;
                    canvas.DrawRect(x, y, tileSize, tileSize, paint);
                }
            }
        }
    }

    private void RenderText(SKCanvas canvas, int size, string text)
    {
        // Background: dark grey
        canvas.Clear(new SKColor(18, 18, 18, 255));

        // Draw text
        using (var paint = new SKPaint())
        {
            paint.Color = new SKColor(240, 240, 240, 255);
            paint.TextSize = Math.Max(12, size / 20);
            paint.IsAntialias = true;
            paint.Typeface = SKTypeface.FromFamilyName("Arial", SKFontStyle.Normal);

            // Limit text length
            string displayText = text.Length > 200 ? text.Substring(0, 200) : text;
            canvas.DrawText(displayText, 8, paint.TextSize + 8, paint);
        }




    }

    // --------------------------------------------------------------------------------------------
    // MARK: JSON Models
    // --------------------------------------------------------------------------------------------

    private class ImageRequest
    {
        public int size { get; set; }
        public string? contents { get; set; }
    }

    private class ImageResponse
    {
        public int width { get; set; }
        public int height { get; set; }
        public string mime { get; set; } = "";
        public string rgba_png_base64 { get; set; } = "";
    }

    // --------------------------------------------------------------------------------------------
    // MARK: Main Entry Point
    // --------------------------------------------------------------------------------------------

    static void Main(string[] args)
    {
        Console.WriteLine("=== RGBA PNG Image Server ===");
        Console.WriteLine("C# equivalent of Image-TestServer.ipynb");
        Console.WriteLine("HTTP server compatible with FastAPI client");
        Console.WriteLine();

        Console.WriteLine("Running Unit Tests...");
        var testLog = new KoreCommon.UnitTest.KoreTestLog();
        KorePlotterUnitTestCenter.RunAllTests(testLog);
        Console.WriteLine("Unit Tests Completed.");
        Console.WriteLine();

        ProgramServer server = new ProgramServer();

        // Start server on port 8001 to avoid conflicts
        server.StartServer("127.0.0.1", 8001);
        string ipAddr = "127.0.0.1";
        int port = 8000;
        server.StartServer(ipAddr, port);

        Console.WriteLine();
        Console.WriteLine("Endpoints:");
        Console.WriteLine($"  GET  http://{ipAddr}:{port}/health");
        Console.WriteLine($"  POST http://{ipAddr}:{port}/render");
        Console.WriteLine();
        Console.WriteLine("Example request body:");
        Console.WriteLine("  {\"size\": 256, \"contents\": \"solid:red\"}");
        Console.WriteLine("  {\"size\": 256, \"contents\": \"checker:black,white,8\"}");
        Console.WriteLine("  {\"size\": 256, \"contents\": \"text:Hello World\"}");
        Console.WriteLine();
        Console.WriteLine("Press Ctrl+C to stop the server...");

        // Handle Ctrl+C gracefully
        Console.CancelKeyPress += (sender, e) =>
        {
            e.Cancel = true;
            server.StopServer();
            Environment.Exit(0);
        };

        // Keep main thread alive
        while (true)
        {
            Thread.Sleep(1000);
        }
    }
}
