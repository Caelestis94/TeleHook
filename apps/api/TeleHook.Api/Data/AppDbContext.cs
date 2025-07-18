using Microsoft.EntityFrameworkCore;
using TeleHook.Api.Models;

namespace TeleHook.Api.Data;

public class AppDbContext : DbContext
{
    private readonly IConfiguration _configuration;
    public DbSet<Bot> Bots { get; set; }
    public DbSet<Webhook> Webhooks { get; set; }
    public DbSet<WebhookLog> WebhookLogs { get; set; }
    public DbSet<WebhookStat> WebhookStats { get; set; }
    public DbSet<User> Users { get; set; }
    public DbSet<AppSetting> AppSettings { get; set; }

    public AppDbContext(IConfiguration configuration)
    {
        _configuration = configuration ?? throw new ArgumentNullException(nameof(configuration));
    }

    public AppDbContext(DbContextOptions<AppDbContext> options, IConfiguration configuration) : base(options)
    {
        _configuration = configuration;
    }

    protected override void OnConfiguring(DbContextOptionsBuilder options)
    {
        if (!options.IsConfigured)
            options.UseSqlite(_configuration.GetConnectionString("DatabaseConnectionString"));
    }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);


        modelBuilder.Entity<Bot>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.BotToken).IsRequired();
            entity.Property(e => e.ChatId).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            entity.HasIndex(e => e.Name).IsUnique();
        });

        modelBuilder.Entity<Webhook>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired();
            entity.Property(e => e.Uuid).IsRequired();
            entity.Property(e => e.MessageTemplate).IsRequired();
            entity.Property(e => e.ParseMode).IsRequired().HasDefaultValue("MarkdownV2");
            entity.Property(e => e.DisableWebPagePreview).HasDefaultValue(true);
            entity.Property(e => e.DisableNotification).HasDefaultValue(false);
            entity.Property(e => e.IsDisabled).HasDefaultValue(true);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.IsProtected).HasDefaultValue(false);
            entity.Property(e => e.SecretKey).HasMaxLength(255).IsRequired(false);
            entity.Property(e => e.PayloadSample).IsRequired().HasDefaultValue("{}");
            entity.HasIndex(e => e.Uuid).IsUnique();

            entity.ToTable(t => t.HasCheckConstraint("CK_Webhooks_ParseMode",
                "ParseMode IN ('HTML', 'Markdown', 'MarkdownV2')"));

            entity.HasOne(e => e.Bot)
                .WithMany()
                .HasForeignKey(e => e.BotId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<Webhook>()
            .HasIndex(e => e.BotId)
            .HasDatabaseName("IX_Webhooks_BotId");

        modelBuilder.Entity<Webhook>()
            .HasIndex(e => e.IsProtected)
            .HasDatabaseName("IX_Webhooks_IsProtected");

        modelBuilder.Entity<Webhook>()
            .HasIndex(e => e.IsDisabled)
            .HasDatabaseName("IX_Webhooks_IsDisabled");

        modelBuilder.Entity<WebhookLog>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.RequestId).IsRequired();
            entity.Property(e => e.HttpMethod).IsRequired().HasDefaultValue("POST");
            entity.Property(e => e.RequestUrl).IsRequired();
            entity.Property(e => e.ResponseStatusCode).IsRequired();
            entity.Property(e => e.ProcessingTimeMs).IsRequired();
            entity.Property(e => e.PayloadValidated).IsRequired();
            entity.Property(e => e.TelegramSent).IsRequired();
            entity.Property(e => e.CreatedAt).IsRequired();

            // Foreign key relationship
            entity.HasOne(e => e.Webhook)
                .WithMany()
                .HasForeignKey(e => e.WebhookId)
                .OnDelete(DeleteBehavior.Cascade);

            // Indexes
            entity.HasIndex(e => e.WebhookId)
                .HasDatabaseName("IX_WebhookLogs_WebhookId");
            entity.HasIndex(e => e.CreatedAt)
                .HasDatabaseName("IX_WebhookLogs_CreatedAt");
            entity.HasIndex(e => e.ResponseStatusCode)
                .HasDatabaseName("IX_WebhookLogs_ResponseStatusCode");
            entity.HasIndex(e => e.RequestId)
                .HasDatabaseName("IX_WebhookLogs_RequestId");
        });

        modelBuilder.Entity<WebhookStat>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Date).IsRequired();
            entity.Property(e => e.TotalRequests).HasDefaultValue(0);
            entity.Property(e => e.SuccessfulRequests).HasDefaultValue(0);
            entity.Property(e => e.FailedRequests).HasDefaultValue(0);
            entity.Property(e => e.ValidationFailures).HasDefaultValue(0);
            entity.Property(e => e.TelegramFailures).HasDefaultValue(0);
            entity.Property(e => e.TotalProcessingTimeMs).HasDefaultValue(0);
            entity.Property(e => e.AvgProcessingTimeMs).HasDefaultValue(0);
            entity.Property(e => e.MinProcessingTimeMs).HasDefaultValue(0);
            entity.Property(e => e.MaxProcessingTimeMs).HasDefaultValue(0);
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            // Unique constraint on Date + WebhookId
            entity.HasIndex(e => new { e.Date, e.WebhookId })
                .IsUnique()
                .HasDatabaseName("IX_WebhookStats_Date_Id");

            // Performance indexes
            entity.HasIndex(e => e.Date)
                .HasDatabaseName("IX_WebhookStats_Date");
            entity.HasIndex(e => e.WebhookId)
                .HasDatabaseName("IX_WebhookStats_WebhookId");

            // Foreign key relationship (nullable)
            entity.HasOne(e => e.Webhook)
                .WithMany()
                .HasForeignKey(e => e.WebhookId)
                .OnDelete(DeleteBehavior.Cascade)
                .IsRequired(false); // Allow NULL for global stats
        });

        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(255);
            entity.Property(e => e.Username).IsRequired().HasMaxLength(255);
            entity.Property(e => e.PasswordHash).HasMaxLength(255);
            entity.Property(e => e.FirstName).HasMaxLength(100);
            entity.Property(e => e.LastName).HasMaxLength(100);
            entity.Property(e => e.Role).IsRequired().HasMaxLength(50).HasDefaultValue("admin");
            entity.Property(e => e.CreatedAt).IsRequired();
            entity.Property(e => e.UpdatedAt).IsRequired();

            entity.Property(e => e.OidcId).HasMaxLength(255);
            entity.Property(e => e.AuthProvider).HasMaxLength(50).HasDefaultValue("credentials");

            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.Username).IsUnique();
            entity.HasIndex(e => e.OidcId).IsUnique();

            entity.ToTable(t => t.HasCheckConstraint("CK_Users_Role",
                "Role IN ('admin', 'user')"));

            entity.ToTable(t => t.HasCheckConstraint("CK_Users_AuthProvider",
                "AuthProvider IN ('credentials', 'oidc')"));

            entity.ToTable(t => t.HasCheckConstraint("CK_Users_AuthMethod",
                "(PasswordHash IS NOT NULL AND AuthProvider = 'credentials') OR " +
                "(OidcId IS NOT NULL AND AuthProvider = 'oidc')"));
        });

        modelBuilder.Entity<AppSetting>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.LogLevel).HasDefaultValue("Warning");
            entity.Property(e => e.LogRetentionDays).HasDefaultValue(7);
            entity.Property(e => e.EnableWebhookLogging).HasDefaultValue(true);
            entity.Property(e => e.EnableFailureNotifications).HasDefaultValue(false);
            entity.Property(e => e.NotificationBotToken).HasDefaultValue(null);
            entity.Property(e => e.NotificationChatId).HasDefaultValue(null);
            entity.Property(e => e.NotificationTopicId).HasDefaultValue(null);
            entity.Property(e => e.WebhookLogRetentionDays).HasDefaultValue(0);
            entity.Property(e => e.StatsDaysInterval).HasDefaultValue(30);
            entity.Property(e => e.LogPath)
                .HasDefaultValue("/app/logs/telehook-.log")
                .HasMaxLength(255);
            entity.Property(e => e.CreatedAt).HasDefaultValueSql("datetime('now', 'utc')");
            entity.Property(e => e.UpdatedAt).HasDefaultValueSql("datetime('now', 'utc')");
            entity.ToTable(t => t.HasCheckConstraint("CK_AppSettings_SingleRow",
                "Id = 1"));
        });

        modelBuilder.Entity<AppSetting>().HasData(new AppSetting
        {
            Id = 1,
            LogLevel = "Warning",
            LogRetentionDays = 7,
            EnableWebhookLogging = true,
            WebhookLogRetentionDays = 0,
            StatsDaysInterval = 30,
        });
    }
}
