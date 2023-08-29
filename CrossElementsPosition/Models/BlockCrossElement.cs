﻿using Autodesk.Revit.DB;
using System;
using System.Collections.Generic;
using System.IO;
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
        public int CountCrossSection { get; set; }

        public BlockCrossElement(Document doc, Element blockElement, List<Element> markupElements)
        {
            BlockElement = blockElement;
            BlockAxis = RevitGeometryUtils.GetBlockAxis(doc, blockElement);
            MarkupElement = RevitGeometryUtils.GetClosestMarkupElement(doc, blockElement, markupElements);
            CountCrossSection = GetCountCrossSection();
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

        public List<XYZ> GetIntersectPointsOnBlock()
        {
            var points = new List<XYZ>();
            var planes = this.GetMarkupPlanes();

            foreach (var plane in planes)
            {
                double parameter;
                XYZ point = LinePlaneIntersection(BlockAxis, plane, out parameter);
                points.Add(point);
            }

            return points;
        }

        public void HideUnusedCrossSection()
        {
            var parameters = BlockElement.Parameters;
            var visibleParameters = new List<(Parameter Parameter, int Number)>();
            foreach(Parameter parameter in parameters)
            {
                string parameterName = parameter.Definition.Name;
                if (parameterName.Contains("Ребро") && parameterName.Contains("Видимость"))
                {
                    var number = int.Parse(parameterName.Split('_').ElementAt(1));
                    visibleParameters.Add((parameter, number));
                }
            }

            visibleParameters = visibleParameters.OrderBy(p => p.Number).ToList();

            for(int i = visibleParameters.Count - 1; i >= CountCrossSection; i--)
            {
                var hideParameter = visibleParameters.ElementAt(i);
                hideParameter.Parameter.Set(0);
            }
        }

        private int GetCountCrossSection()
        {
            Options options = new Options();
            var geometryInstance = MarkupElement.get_Geometry(options).First() as GeometryInstance;
            var lines = geometryInstance.GetInstanceGeometry().OfType<Line>();

            return lines.Count();
        }

        private List<Plane> GetMarkupPlanes()
        {
            Options options = new Options();
            var geometryInstance = MarkupElement.get_Geometry(options).First() as GeometryInstance;
            var lines = geometryInstance.GetInstanceGeometry().OfType<Line>();

            var planes = lines.Select(l => GetPlaneByLine(l)).ToList();

            return planes;
        }

        private static Plane GetPlaneByLine(Line line)
        {
            XYZ firstPoint = line.GetEndPoint(0);
            XYZ secondPoint = line.GetEndPoint(1);
            XYZ thirdPoint = firstPoint + XYZ.BasisZ;

            Plane plane = Plane.CreateByThreePoints(firstPoint, secondPoint, thirdPoint);

            return plane;
        }

        /* Пересечение линии и плоскости
        * (преобразует линию в вектор, поэтому пересекает любую линию не параллельную плоскости)
        */
        private static XYZ LinePlaneIntersection(Line line, Plane plane, out double lineParameterNormalized)
        {
            XYZ planePoint = plane.Origin;
            XYZ planeNormal = plane.Normal;
            XYZ linePoint = line.GetEndPoint(0);
            double lineParameter = double.NaN;
            lineParameterNormalized = double.NaN;

            XYZ lineDirection = (line.GetEndPoint(1) - linePoint).Normalize();

            // Проверка на параллельность линии и плоскости
            if ((planeNormal.DotProduct(lineDirection)) == 0)
            {
                lineParameter = double.NaN;
                return null;
            }

            lineParameter = (planeNormal.DotProduct(planePoint)
              - planeNormal.DotProduct(linePoint))
                / planeNormal.DotProduct(lineDirection);

            lineParameterNormalized = line.ComputeNormalizedParameter(lineParameter);

            return linePoint + lineParameter * lineDirection;
        }
    }
}
