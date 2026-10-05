using Microsoft.EntityFrameworkCore;
using RestaurantAPI.Entities;
using System.Collections.Generic;
using System.Linq;

namespace RestaurantAPI
{
    public class RestaurantSeeder
    {
        private readonly RestaurantDbContext _dbContext;
        public RestaurantSeeder(RestaurantDbContext dbContext)
        {
            _dbContext = dbContext;
        }
        public void Seed()
        {
            if (_dbContext.Database.CanConnect())
            {
                //sprawdzamy aktualnosc danych
                var pendingMigrations = _dbContext.Database.GetPendingMigrations(); //<--- zwroci migracje ktore nie zostaly jeszcze zaaplikowane
                
                if(pendingMigrations != null && pendingMigrations.Any()) // jakasmigracja nie zostala zaaplikwoana na bazie danych jesli tak
                {
                    _dbContext.Database.Migrate();
                }
                if(!_dbContext.Roles.Any())
                {
                    var roles = GetRoles();
                    _dbContext.Roles.AddRange(roles);
                    _dbContext.SaveChanges();
                }
                if (!_dbContext.Restaurants.Any())
                {
                    var restaurants = GetRestaurants();
                    _dbContext.Restaurants.AddRange(restaurants);
                    _dbContext.SaveChanges();
                }
            }
        }
        private IEnumerable<Role> GetRoles()
        {
            var roles = new List<Role>()
            {
                new Role()
                {
                    Name = "User"
                },
                new Role()
                {
                    Name = "Manager"
                },
                new Role()
                {
                    Name = "Admin"
                }
            };
            return roles;
        }
        private IEnumerable<Restaurant> GetRestaurants()
        {
            var restaurants = new List<Restaurant>()
    {
        new Restaurant()
        {
            Name = "KFC",
            Category = "Fast Food",
            Description = "KFC (short for Kentucky Fried Chicken) is an American fast food restaurant chain headquartered in Louisville, Kentucky, that specializes in fried chicken.",
            ContactEmail = "contact@kfc.com",
            HasDelivery = true, // Poprawiono: bool zamiast "true"
            Dishes = new List<Dish>()
            {
                new Dish()
                {
                    Name = "Nashville Hot Chicken",
                    Price = 10.30M,
                },
                new Dish()
                {
                    Name = "Chicken Nuggets",
                    Price = 5.30M,
                },
            },
            Address = new Address()
            {
                City = "Kraków",
                Street = "Długa 5",
                PostalCode = "30-001"
            }
        },
        new Restaurant()
        {
            Name = "McDonald",
            Category = "Fast Food",
            Description = "McDonald's is an American fast food company.",
            ContactEmail = "contact@mcdonald.com",
            HasDelivery = false, 
            Dishes = new List<Dish>()
            {
                new Dish()
                {
                    Name = "Fries",
                    Price = 10.30M,
                }
            },
            
            Address = new Address()
            {
                City = "Kraków",
                Street = "Szewska 2",
                PostalCode = "31-009"
            }
        }
    };

            return restaurants;
        }
    }
}
