using Drivetrain.Robotics.Control;
using Effani.Device.DataCollection;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Effani.Device.Services
{
	public class EfContext : DbContext
	{
		#region constructors
		public EfContext() : base() { }
		public EfContext(DbContextOptions options) : base(options)
		{
		}
		#endregion

		#region protected methods
		protected override void ConfigureConventions(ModelConfigurationBuilder configurationBuilder)
		{
			configurationBuilder
				.Properties<Coordinate>()
				.HaveConversion<CoordinateConverter>();
		}
		#region OnConfiguring
		/// <summary>
		/// <para> /// Override this method to configure the database (and other options) to be used for this context.
		/// /// This method is called for each instance of the context that is created. /// The base implementation does
		/// nothing. ///</para> <para> /// In situations where an instance of <see
		/// cref="Microsoft.EntityFrameworkCore.DbContextOptions"/> may or may not have been passed /// to the
		/// constructor, you can use <see cref="Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.IsConfigured"/> to
		/// determine if /// the options have already been set, and skip some or all of the logic in /// <see
		/// cref="M:Microsoft.EntityFrameworkCore.DbContext.OnConfiguring(Microsoft.EntityFrameworkCore.DbContextOptionsBuilder)"/>.
		/// ///</para>
		/// </summary>
		/// <param name="optionsBuilder">
		/// A builder used to create or modify options for this context. Databases (and other extensions) typically
		/// define extension methods on this object that allow you to configure the context.
		/// </param>
		protected override void OnConfiguring(DbContextOptionsBuilder optionsBuilder)
		{
			optionsBuilder
				.UseSqlite($"Data Source={Path.Combine(SettingsService.DataFolder, "AssayData.effani")}");
		}
		#endregion
		#region OnModelCreating
		/// <summary>
		/// Override this method to further configure the model that was discovered by convention from the entity types
		/// exposed in <see cref="Microsoft.EntityFrameworkCore.DbSet{TEntity}"/> properties on your derived context.
		/// The resulting model may be cached and re-used for subsequent instances of your derived context.
		/// </summary>
		/// <remarks>
		/// If a model is explicitly set on the options for this context (via <see ///
		/// cref="M:Microsoft.EntityFrameworkCore.DbContextOptionsBuilder.UseModel(Microsoft.EntityFrameworkCore.Metadata.IModel)"/>)
		/// then this method will not be run.
		/// </remarks>
		/// <param name="modelBuilder">
		/// The builder being used to construct the model for this context. Databases (and other extensions) typically
		/// define extension methods on this object that allow you to configure aspects of the model that are specific
		/// to a given database.
		/// </param>
		protected override void OnModelCreating(ModelBuilder modelBuilder)
		{
			base.OnModelCreating(modelBuilder);

			// The best way to debug migration creations is by enabling the Debugger.Launch() below
			//Debugger.Launch();

			modelBuilder.Entity<SettingsVersion>().Property(e => e.DeviceSettings).HasConversion(
				v => JsonSerializer.Serialize(v, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }),
				v => JsonSerializer.Deserialize<Settings>(v, new JsonSerializerOptions { DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull }));
		}
		#endregion
		#endregion

		#region properties
		public DbSet<AssayData> AssayData { get; set; }
		public DbSet<SettingsVersion> SettingsVersions { get; set; }
		#endregion
	}
	public class CoordinateConverter : ValueConverter<Coordinate, string>
	{
		public CoordinateConverter() : base(convertToProviderExpression, convertFromProviderExpression, null)
		{ }

		public static string ToString(Coordinate obj)
		{
			return JsonSerializer.Serialize(obj);
		}

		public static Coordinate ToObject(string objString)
		{
			return JsonSerializer.Deserialize<Coordinate>(objString);
		}

		private static Expression<Func<Coordinate, string>> convertToProviderExpression = x => ToString(x);
		private static Expression<Func<string, Coordinate>> convertFromProviderExpression = x => ToObject(x);
	}
}
