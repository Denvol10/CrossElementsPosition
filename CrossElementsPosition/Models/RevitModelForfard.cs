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
            BlockElements = RevitGeometryUtils.GetElementsBySelection(Uiapp, out _blockElementIds);
        }
        #endregion


    }
}
