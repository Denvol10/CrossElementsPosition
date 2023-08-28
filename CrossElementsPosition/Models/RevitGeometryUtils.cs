using Autodesk.Revit.DB;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.UI;
using CrossElementsPosition.Models.Filters;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Documents;

namespace CrossElementsPosition.Models
{
    public class RevitGeometryUtils
    {
        // Получение блоков пролетного строения с помощью пользовательского выбора
        public static List<Element> GetElementsBySelection(UIApplication uiapp, ISelectionFilter filter, out string elementIds)
        {
            Selection sel = uiapp.ActiveUIDocument.Selection;
            var pickedElems = sel.PickElementsByRectangle(filter, "Select Blocks");
            elementIds = ElementIdToString(pickedElems);

            return pickedElems.ToList();
        }

        // Метод получения строки с ElementId
        private static string ElementIdToString(IEnumerable<Element> elements)
        {
            var stringArr = elements.Select(e => "Id" + e.Id.IntegerValue.ToString()).ToArray();
            string resultString = string.Join(", ", stringArr);

            return resultString;
        }


        // Проверка на то существуют ли элементы с данным Id в модели
        public static bool IsElemsExistInModel(Document doc, IEnumerable<int> elems, Type type)
        {
            if (elems is null)
            {
                return false;
            }

            foreach (var elem in elems)
            {
                ElementId id = new ElementId(elem);
                Element curElem = doc.GetElement(id);
                if (curElem is null || !(curElem.GetType() == type))
                {
                    return false;
                }
            }

            return true;
        }

        // Получение блоков по их id
        public static List<Element> GetElementsById(Document doc, IEnumerable<int> ids)
        {
            var elems = new List<Element>();
            foreach (var id in ids)
            {
                ElementId elemId = new ElementId(id);
                Element block = doc.GetElement(elemId);
                elems.Add(block);
            }

            return elems;
        }

        // Получение id элементов на основе списка в виде строки
        public static List<int> GetIdsByString(string elems)
        {
            if (string.IsNullOrEmpty(elems))
            {
                return null;
            }

            var elemIds = elems.Split(new char[] { ',', ' ' }, StringSplitOptions.RemoveEmptyEntries)
                         .Select(s => int.Parse(s.Remove(0, 2)))
                         .ToList();

            return elemIds;
        }

        public static double GetDistanceBetweenElements(Element blockElem, Element markupElem, Document doc)
        {
            // Получение центральной точки для блока
            Line blockAxis = GetBlockAxis(doc, blockElem);
            XYZ blockCentralPoint = blockAxis.Evaluate(0.5, true);

            // Получение центральной точки для элемента разметки
            var markupLocation = markupElem.Location as LocationCurve;
            Curve markupCurve = markupLocation.Curve;
            XYZ markupCentralPoint = markupCurve.Evaluate(0.5, true);

            using(Transaction trans = new Transaction(doc, "Create Test Points"))
            {
                trans.Start();
                doc.FamilyCreate.NewReferencePoint(blockCentralPoint);
                trans.Commit();
            }

            return blockCentralPoint.DistanceTo(markupCentralPoint);
        }

        private static Line GetBlockAxis(Document doc, Element blockElem)
        {
            var blockAdaptivePoints = AdaptiveComponentInstanceUtils.GetInstancePlacementPointElementRefIds(blockElem as FamilyInstance);
            var firstReferencePoint = doc.GetElement(blockAdaptivePoints.FirstOrDefault()) as ReferencePoint;
            var secondReferencePoint = doc.GetElement(blockAdaptivePoints.ElementAt(1)) as ReferencePoint;
            XYZ firstPoint = firstReferencePoint.Position;
            XYZ secondPoint = secondReferencePoint.Position;

            XYZ alongVector = (secondPoint - firstPoint).Normalize();
            Parameter lengthParameter = blockElem.LookupParameter("Длина блока");
            double length = lengthParameter.AsDouble();
            XYZ secondBlockCurvePoint = firstPoint + alongVector * length;

            Line blockAxis = Line.CreateBound(firstPoint, secondBlockCurvePoint);

            return blockAxis;
        }
    }
}
