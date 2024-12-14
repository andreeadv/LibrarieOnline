using Microsoft.EntityFrameworkCore;
using LibrarieOnline.Models;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;

namespace LibrarieOnline.Data
{
    public class LibrarieOnlineContext : IdentityDbContext<ApplicationUser>
    {
        // Constructorul contextului, care permite configurarea opțională
        public LibrarieOnlineContext(DbContextOptions<LibrarieOnlineContext> options)
            : base(options)
        {
        }

        // Proprietăți DbSet pentru fiecare model
        public DbSet<BookModel> Books { get; set; }
        public DbSet<CartModel> Carts { get; set; }
        public DbSet<CategoryModel> Categories { get; set; }
        public DbSet<CommentModel> Comments { get; set; }
        public DbSet<QuizModel> Quizzes { get; set; }
        public DbSet<QuestionQuizModel> QuestionQuizzes { get; set; }
        public DbSet<RewardModel> Rewards { get; set; }
        public DbSet<BookCartModel> BookCarts { get; set; }
        public DbSet<OrderModel> Orders { get; set; }
        public DbSet<UserRewardModel> UserRewards { get; set; }
        public DbSet<ApplicationUser> ApplicationUsers { get; set; }

        // Configurarea relațiilor și comportamentului de bază
        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            // Relația între Book și Category
            modelBuilder.Entity<BookModel>()
                .HasOne(b => b.Category)
                .WithMany(c => c.Books)
                .HasForeignKey(b => b.CategoryID);

            // Relația între Comment și Book
            modelBuilder.Entity<CommentModel>()
                .HasOne(c => c.Book)
                .WithMany(b => b.Comments)
                .HasForeignKey(c => c.BookID);

            // Relația între Comment și User
            modelBuilder.Entity<CommentModel>()
                .HasOne(c => c.User)
                .WithMany(u => u.Comments)
                .HasForeignKey(c => c.UserId);

            // Relația 1 la 1 între Cart și User
            modelBuilder.Entity<CartModel>()
                .HasOne(c => c.User)
                .WithOne(u => u.Cart)
                .HasForeignKey<CartModel>(c => c.UserId)
                .OnDelete(DeleteBehavior.NoAction);  // Avoid cascade delete

            modelBuilder.Entity<OrderModel>()
                .HasOne(o => o.Cart)          // Un Order are un Cart
                .WithOne(c => c.Order)        // Un Cart are un Order
                .HasForeignKey<OrderModel>(o => o.CartID) // Cheia străină în Order
                .OnDelete(DeleteBehavior.NoAction);

            // Relația Many-to-Many între Book și Cart prin BookCart
            modelBuilder.Entity<BookCartModel>()
                .HasKey(bc => new { bc.BookID, bc.CartID });

            modelBuilder.Entity<BookCartModel>()
                .HasOne(bc => bc.Book)
                .WithMany(b => b.BookCarts)
                .HasForeignKey(bc => bc.BookID);

            modelBuilder.Entity<BookCartModel>()
                .HasOne(bc => bc.Cart)
                .WithMany(c => c.BookCarts)
                .HasForeignKey(bc => bc.CartID);

            // Relația între Order și User (un User poate face mai multe comenzi)
            modelBuilder.Entity<OrderModel>()
                .HasOne(o => o.User)
                .WithMany(u => u.Orders)
                .HasForeignKey(o => o.UserId)
                .OnDelete(DeleteBehavior.NoAction);


            // Relația între UserReward și User
            modelBuilder.Entity<UserRewardModel>()
                .HasOne(ur => ur.User)
                .WithMany(u => u.UserRewards)
                .HasForeignKey(ur => ur.UserId);

            // Relația între UserReward și Reward
            modelBuilder.Entity<UserRewardModel>()
                .HasOne(ur => ur.Reward)
                .WithMany(r => r.UserRewards)
                .HasForeignKey(ur => ur.RewardID);

            // Relația între Quiz și Reward
            modelBuilder.Entity<QuizModel>()
                .HasOne(q => q.Reward)
                .WithMany(r => r.Quizzes)
                .HasForeignKey(q => q.RewardID);

            // Relația între QuestionQuiz și Quiz
            modelBuilder.Entity<QuestionQuizModel>()
                .HasOne(q => q.Quiz)
                .WithMany(qz => qz.QuestionQuizzes)
                .HasForeignKey(q => q.QuizID);
        }
    }
}
