using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Services.Databases;

public class ContextFactory
{
    public static T CreateContext<T>(string connectionString) where T : DbContext
    {
        var options = new DbContextOptionsBuilder<T>()
            .UseMySql(connectionString, ServerVersion.AutoDetect(connectionString))
            .Options;

        return (T)Activator.CreateInstance(typeof(T), options);
    }
}
