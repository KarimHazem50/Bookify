using BookifyTest.Core.Mapping;
using BookifyTest.Helper;
using BookifyTest.Settings;
using Hangfire;
using HashidsNet;
using Microsoft.AspNetCore.DataProtection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.UI.Services;
using System.Reflection;
using UoN.ExpressiveAnnotations.NetCore.DependencyInjection;
using ViewToHTML.Extensions;
using WhatsAppCloudApi.Extensions;

namespace BookifyTest.Extensions
{
    public static class DependancyInjection
    {
        public static IServiceCollection AddBookifyServices(this IServiceCollection services, WebApplicationBuilder builder)
        {
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            builder.Services.AddDbContext<ApplicationDbContext>(options =>
                options.UseSqlServer(connectionString!));

            builder.Services.AddDatabaseDeveloperPageExceptionFilter();

            builder.Services.AddIdentity<ApplicationUser, IdentityRole>(options => options.SignIn.RequireConfirmedAccount = true)
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultUI()
                .AddDefaultTokenProviders();

            builder.Services.Configure<IdentityOptions>(options =>
            {
                options.Password.RequiredLength = 8;
                options.User.RequireUniqueEmail = true;
                options.Lockout.DefaultLockoutTimeSpan = TimeSpan.FromMinutes(10);
            });

            builder.Services.AddDataProtection().SetApplicationName(nameof(BookifyTest));

            builder.Services.AddSingleton<IHashids>(_ => new Hashids(minHashLength: 5));

            builder.Services.AddScoped<IUserClaimsPrincipalFactory<ApplicationUser>, ApplicationUserClaimsPrincipalFactory>();

            builder.Services.AddTransient<IImageService, ImageService>();
            builder.Services.AddTransient<IEmailSender, EmailSender>();
            builder.Services.AddTransient<IEmailBodyBuilder, EmailBodyBuilder>();

            builder.Services.AddControllersWithViews();

            builder.Services.AddAutoMapper(Assembly.GetAssembly(typeof(MappingProfile)));
            builder.Services.AddExpressiveAnnotations();
            builder.Services.Configure<MailSettings>(builder.Configuration.GetSection(nameof(MailSettings)));

            builder.Services.Configure<SecurityStampValidatorOptions>(options => options.ValidationInterval = TimeSpan.Zero);

            builder.Services.AddWhatsAppApiClient(builder.Configuration);

            builder.Services.AddHangfire(x => x.UseSqlServerStorage(connectionString));
            builder.Services.AddHangfireServer();

            builder.Services.Configure<AuthorizationOptions>(options => options.AddPolicy("AdminsOnly", policy =>
            {
                policy.RequireAuthenticatedUser();
                policy.RequireRole(AppRoles.Admin);
            }));

            builder.Services.AddViewToHTML();

            builder.Services.AddMvc(options =>
                options.Filters.Add(new AutoValidateAntiforgeryTokenAttribute())
            );


            return services;
        }
    }
}
