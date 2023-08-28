using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossElementsPosition.Models.Filters
{
    public class FurnitureCategoryFilter : ISelectionFilter
    {
        public bool AllowElement(Element elem)
        {
            int elemCategoryId = elem.Category.Id.IntegerValue;
            int furnitureCategoryId = (int)BuiltInCategory.OST_Furniture;

            if (elemCategoryId == furnitureCategoryId)
            {
                return true;
            }

            return false;
        }

        public bool AllowReference(Reference reference, XYZ position)
        {
            return true;
        }
    }
}
