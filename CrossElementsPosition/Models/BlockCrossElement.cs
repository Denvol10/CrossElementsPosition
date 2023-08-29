using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CrossElementsPosition.Models
{
    public class BlockCrossElement
    {

        public Element BlockElement { get; set; }
        public Line BlockAxis { get; set; }
        public Element MarkupElement { get; set; }

        public BlockCrossElement(Document doc, Element blockElement, List<Element> markupElements)
        {
            BlockElement = blockElement;
            BlockAxis = RevitGeometryUtils.GetBlockAxis(doc, blockElement);
            MarkupElement = RevitGeometryUtils.GetClosestMarkupElement(doc, blockElement, markupElements);
        }

        public XYZ GetBlockCentralPoint()
        {
            XYZ centralPoint = BlockAxis.Evaluate(0.5, true);

            return centralPoint;
        }

        public XYZ GetMarkupCentralPoint()
        {
            var markupLocation = MarkupElement.Location as LocationCurve;
            Curve markupCurve = markupLocation.Curve;
            XYZ markupCentralPoint = markupCurve.Evaluate(0.5, true);

            return markupCentralPoint;
        }
    }
}
