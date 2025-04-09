using System;
using System.Collections.Generic;
using System.Linq;
using System.Resources;
using System.Text;
using System.Threading.Tasks;

namespace KUtilitiesCore.Helpers
{
    public class ResourceHelpers
    {
        /// <summary>
        /// Obtiene acceso a un recurso del proyecto a travez del Tipo
        /// </summary>
        /// <typeparam name="TResult"></typeparam>
        /// <param name="ResourceType">El tipo que representa el Recurso en el proyecto</param>
        /// <param name="AccessorFunc">Expresion de acceso al recurso por medio del <see cref="ResourceManager"/></param>
        /// <returns></returns>
        public static TResult GetFromResource<TResult>(Type ResourceType, Func<ResourceManager, TResult> AccessorFunc)
        {
            ResourceManager res = new ResourceManager(ResourceType);
            return AccessorFunc.Invoke(res);
        }
    }
}
