using test3.Repo.AccountRepo;

var builder = WebApplication.CreateBuilder(args);
builder.Configuration
    .AddJsonFile("appsettings.Local.json", optional: true, reloadOnChange: true)
    .AddEnvironmentVariables();
var  MyAllowSpecificOrigins = "_myAllowSpecificOrigins";
// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddDistributedMemoryCache();
builder.Services.AddSession(options =>
{
    options.IdleTimeout = TimeSpan.FromMinutes(990);
    options.Cookie.HttpOnly = true;
    options.Cookie.IsEssential = true;
});
builder.Services.AddScoped<IAccountRepository,AccountRepository>();
builder.Services.AddScoped<IMedicationRepository,MedicationRepository>();
builder.Services.AddScoped<IUsageLogRepo, UsageLogRepo>();
builder.Services.AddScoped<ISmartBoxRepo, SmartBoxRepo>();
builder.Services.AddScoped<IPrescriptionRepository, PrescriptionRepository>();
builder.Services.AddScoped<IMailingHelper, MailingHelper>();
builder.Services.AddScoped<IEnvLogRepo, EnvLogRepo>();
builder.Services.AddScoped<IReminderRepo, ReminderRepo>();
builder.Services.AddScoped<ITestRemindRepo, TestRemindRepo>();
builder.Services.AddScoped<ISmartBoxMedicationRepo, SmartBoxMedicationRepo>();
// builder.Services.AddScoped<IAccountFirebaseRepo, AccountFirebaseRepo>();

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: MyAllowSpecificOrigins,
                      policy  =>
                      {
                          policy.WithOrigins("http://localhost:5173").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                          policy.WithOrigins("http://localhost:8000").AllowAnyHeader().AllowAnyMethod().AllowCredentials();
                      });
});
var app = builder.Build();

// Configure the HTTP request pipeline.
app.UseSwagger();
app.UseSwaggerUI();
app.UseRouting(); // 添加這一行
app.UseAuthorization();

app.MapControllers();
app.UseSession();
app.UseCors(MyAllowSpecificOrigins);
app.UseStaticFiles();
app.UseEndpoints(endpoints =>
{
    endpoints.MapControllers();
    // Vue 需要這一行來處理未匹配的路由
    endpoints.MapFallbackToFile("index.html");
});

app.Run();
