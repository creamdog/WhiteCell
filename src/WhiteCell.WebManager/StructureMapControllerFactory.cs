using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Web.Mvc;
using StructureMap;

namespace WhiteCell.WebManager
{
    public class StructureMapControllerFactory : DefaultControllerFactory
    {
        protected override IController GetControllerInstance(System.Web.Routing.RequestContext requestContext, Type controllerType)
        {
            if (controllerType == null)
                return null;
            return (IController)ObjectFactory.GetInstance(controllerType);
        }
    }
}