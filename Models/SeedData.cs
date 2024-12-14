using LibrarieOnline.Data;
using LibrarieOnline.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Collections.Generic;

namespace AplicatieMagazinOnline.Models
{
    public static class SeedData
    {
        public static void Initialize(IServiceProvider
       serviceProvider)
        {
            using (var context = new LibrarieOnlineContext(
            serviceProvider.GetRequiredService
            <DbContextOptions<LibrarieOnlineContext>>()))
            {
                // Verificam daca in baza de date exista cel putin un rol
                // insemnand ca a fost rulat codul 
                // De aceea facem return pentru a nu insera rolurile inca o data
                // Acesta metoda trebuie sa se execute o singura data 
                if (context.Roles.Any())
                {
                    return; // baza de date contine deja roluri
                }
                // CREAREA ROLURILOR IN BD
                // daca nu contine roluri, acestea se vor crea
                context.Roles.AddRange(
                new IdentityRole { Id = "cf53f42c-bc77-4a82-81cc-aa492213d610", Name = "Admin", NormalizedName = "Admin".ToUpper() },
            
                new IdentityRole { Id = "cf53f42c-bc77-4a82-81cc-aa492213d612", Name = "User", NormalizedName = "User".ToUpper() }
                );
                // o noua instanta pe care o vom utiliza pentru crearea parolelor utilizatorilor
                // parolele sunt de tip hash
                var hasher = new PasswordHasher<ApplicationUser>();
                // CREAREA USERILOR IN BD
                // Se creeaza cate un user pentru fiecare rol
                context.Users.AddRange(
                new ApplicationUser
                {
                    Id = "18a169b2-a85d-468c-af76-194f243ba010",
                    // primary key
                    UserName = "admin@test.com",
                    EmailConfirmed = true,
                    NormalizedEmail = "ADMIN@TEST.COM",
                    Email = "admin@test.com",
                    NormalizedUserName = "ADMIN@TEST.COM",
                    PasswordHash = hasher.HashPassword(null,
               "Admin1!")
                },

               new ApplicationUser
               {
                   Id = "18a169b2-a85d-468c-af76-194f243ba012",
                   // primary key
                   UserName = "user@test.com",
                   EmailConfirmed = true,
                   NormalizedEmail = "USER@TEST.COM",
                   Email = "user@test.com",
                   NormalizedUserName = "USER@TEST.COM",
                   PasswordHash = hasher.HashPassword(null,
               "User1!")
               }
               );
                // ASOCIEREA USER-ROLE
                context.UserRoles.AddRange(
                new IdentityUserRole<string>
                {
                    RoleId = "cf53f42c-bc77-4a82-81cc-aa492213d610",
                    UserId = "18a169b2-a85d-468c-af76-194f243ba010"
                },
               new IdentityUserRole<string>
               {
                   RoleId = "cf53f42c-bc77-4a82-81cc-aa492213d612",
                   UserId = "18a169b2-a85d-468c-af76-194f243ba012"
               }
                );
                context.SaveChanges();
            }
        }
    }

}
