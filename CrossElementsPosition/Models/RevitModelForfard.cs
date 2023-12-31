﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Autodesk.Revit.ApplicationServices;
using Autodesk.Revit.Attributes;
using Autodesk.Revit.DB;
using Autodesk.Revit.UI;
using Autodesk.Revit.UI.Selection;
using Autodesk.Revit.DB.Architecture;
using System.Collections.ObjectModel;
using CrossElementsPosition.Models;
using CrossElementsPosition.Models.Filters;
using System.IO;

namespace CrossElementsPosition
{
    public class RevitModelForfard
    {
        private UIApplication Uiapp { get; set; } = null;
        private Application App { get; set; } = null;
        private UIDocument Uidoc { get; set; } = null;
        private Document Doc { get; set; } = null;

        public RevitModelForfard(UIApplication uiapp)
        {
            Uiapp = uiapp;
            App = uiapp.Application;
            Uidoc = uiapp.ActiveUIDocument;
            Doc = uiapp.ActiveUIDocument.Document;
        }

        #region Блоки пролетного строения
        public List<Element> BlockElements { get; set; }

        private string _blockElementIds;
        public string BlockElementIds
        {
            get => _blockElementIds;
            set => _blockElementIds = value;
        }

        public void GetBlockElementsBySelection()
        {
            BlockElements = RevitGeometryUtils.GetElementsBySelection(Uiapp, new GenericModelCategoryFilter(), out _blockElementIds);
        }
        #endregion

        // Проверка на то существуют ли блоки в модели
        public bool IsElementsExistInModel(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);

            return RevitGeometryUtils.IsElemsExistInModel(Doc, elemIds, typeof(FamilyInstance));
        }

        // Получение блоков из Settings
        public void GetBlocksBySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            BlockElements = RevitGeometryUtils.GetElementsById(Doc, elemIds);
        }

        #region Элементы разметки
        public List<Element> MarkupElements { get; set; }

        private string _markupElementIds;
        public string MarkupElementIds
        {
            get => _markupElementIds;
            set => _markupElementIds = value;
        }

        public void GetMarkupElementsBySelection()
        {
            MarkupElements = RevitGeometryUtils.GetElementsBySelection(Uiapp, new FurnitureCategoryFilter(), out _markupElementIds);
        }
        #endregion

        // Получение элементов разметки из Settings
        public void GetMarkupElementsBySettings(string elemIdsInSettings)
        {
            var elemIds = RevitGeometryUtils.GetIdsByString(elemIdsInSettings);
            MarkupElements = RevitGeometryUtils.GetElementsById(Doc, elemIds);
        }

        #region Получение положения поперечных элементов
        public void GetCrossElementsPosition()
        {
            var blockCrossElements = new List<BlockCrossElement>();
            foreach (var block in BlockElements)
            {
                var blockCrossElement = new BlockCrossElement(Doc, block, MarkupElements);
                blockCrossElements.Add(blockCrossElement);
            }

            using (Transaction trans = new Transaction(Doc, "Set Cross Section Position"))
            {
                trans.Start();
                foreach(var blockCrossElem in blockCrossElements)
                {
                    blockCrossElem.HideUnusedCrossSection();
                    blockCrossElem.SetCrossSectionParameters();
                }
                trans.Commit();
            }
        }
        #endregion
    }
}
