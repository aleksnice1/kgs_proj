using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Web.Http;
using Kadastr.ServiceLayer.DTO.EntitySearch;
using Kadastr.ServiceLayer.Service;

namespace Kadastr.WebApp.Controllers
{
	public class EntitySearchController : ApiController
	{
		// get /api/EntitySearch/id
		public Entity GetEntityById(Guid id)
		{
			return EntitySearchService.GetByIdentity(id);
		}

		// get /api/EntitySearch?name=category&testAttribute=category&test2=dhjkh
		public IEnumerable<Entity> GetEntityByAttributes(string name)
		{
			var parameters = Request.GetQueryNameValuePairs()
				.Where(item => item.Key.ToLower() != "name")
				.ToDictionary(item => item.Key, item => item.Value);
			return EntitySearchService.FindByAttribute(name, parameters);
		}
	}
}