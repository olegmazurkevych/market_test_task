﻿using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using System;
using System.Linq;
using System.Reflection;
using MarketTest.DAL.Entites;

namespace MarketTest.DAL.Contexts
{
    public class MarketContext : DbContext
    {
        public MarketContext() { }
        
        public MarketContext(DbContextOptions<MarketContext> options)
                    : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            RegisterFoundEntityConfigs(modelBuilder);
            
            base.OnModelCreating(modelBuilder);
        }

        private void RegisterFoundEntityConfigs(ModelBuilder mb)
        {
            AutoRegisterEntityConfigs(mb);

            foreach (var relationship in mb.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            {
                relationship.DeleteBehavior = DeleteBehavior.Restrict;
            }
        }

        private void AutoRegisterEntityConfigs(ModelBuilder mb)
        {
            var entityConfigTypes = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(type => !string.IsNullOrEmpty(type.Namespace))
                .Where(type => type.GetInterfaces()
                                        .FirstOrDefault(y => y.IsGenericType && y.GetGenericTypeDefinition().UnderlyingSystemType == typeof(IEntityTypeConfiguration<>)) != null);

            MethodInfo genericAddConfig = typeof(ModelBuilder).GetMethod(nameof(ModelBuilder.ApplyConfiguration));

            foreach (Type configurationType in entityConfigTypes)
            {
                Type entityType = configurationType.GetTypeInfo().ImplementedInterfaces.First().GetGenericArguments().First();
                object configurationInstance = Activator.CreateInstance(configurationType);
                MethodInfo concreteAddConfig = genericAddConfig.MakeGenericMethod(entityType);
                concreteAddConfig.Invoke(mb, new object[] { configurationInstance });
            }
        }
    }
}
