

var builder = WebApplication.CreateBuilder(args);

// افزودن سرویس YARP
builder.Services.AddReverseProxy()
    .LoadFromConfig(builder.Configuration.GetSection("ReverseProxy"));

var app = builder.Build();

// میدلورهای ضروری
//app.UseHttpsRedirection(); // اگر ssl فعال است، این خط را فعال کنید
app.UseRouting();

// فعال‌سازی YARP
app.MapReverseProxy();

app.Run();

