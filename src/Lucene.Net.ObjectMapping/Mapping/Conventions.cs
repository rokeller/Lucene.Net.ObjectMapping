using Newtonsoft.Json.Linq;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Lucene.Net.Mapping
{
	/// <summary>
	/// Mapping conventions are defined in this class. Essentially,
	/// those are kind of "rules" that define how objects will be mapped to Lucene documents
	/// </summary>
	public class Conventions
	{
		private Func<string, JTokenType, bool> _shouldFieldBeAnalyzed;

		/// <summary>
		/// should set the specified field to be analyzed? field name and its type
		/// are used to decide whether it should be analyzed or not
		/// </summary>
		public Func<string, JTokenType, bool> ShouldStringFieldBeAnalyzed
		{
			get
			{
				return _shouldFieldBeAnalyzed ?? Default.ShouldStringFieldBeAnalyzed;
			}
			set
			{
				_shouldFieldBeAnalyzed = value;
			}
		}

		private Func<string, JTokenType, bool> _shouldFieldBeStored;

		/// <summary>
		/// should set the specified field to be stored? field name and its type
		/// are used to decide whether it should be stored or not
		/// </summary>
		public Func<string, JTokenType, bool> ShouldStringFieldBeStored
		{
			get
			{
				return _shouldFieldBeStored ?? Default.ShouldStringFieldBeStored;
			}
			set
			{
				_shouldFieldBeStored = value;
			}
		}

		private static Conventions _default;
		private static readonly object _syncObj = new object();
		
		/// <summary>
		/// Default conventions for mapping
		/// </summary>
		public static readonly Conventions Default = new Conventions
		{
			ShouldStringFieldBeAnalyzed = (fieldName, fieldType) =>
			{
				if (fieldType == JTokenType.String ||
					fieldType == JTokenType.Uri)
					return true;

				return false;
			},

			ShouldStringFieldBeStored = (fieldName, fieldType) =>
			{
				return false;
			},
		};
	}
}
