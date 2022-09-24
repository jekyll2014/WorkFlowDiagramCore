// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
using Microsoft.Msagl.Drawing;

using Color = Microsoft.Msagl.Drawing.Color;

namespace WorkFlowDiagram
{
    public class State
    {
        public string Id = Guid.NewGuid().ToString();
        public string Label = "";
        public ShapeTag Tag = new ShapeTag();
        //if state was not defined in the file but found in the links
        public bool Orphan = false;
        public List<StateButton> StateButtons = new List<StateButton>();
        public List<StateAction> StateActions = new List<StateAction>();
    }

    public class StateButton
    {
        public string Id = Guid.NewGuid().ToString();
        public string Label = "";
        public List<StateMethod> Methods = new List<StateMethod>();
        public List<StateButton> InternalButtons = new List<StateButton>();
        public ShapeTag Tag = new ShapeTag();
    }

    public class StateAction
    {
        public string Id = Guid.NewGuid().ToString();
        public string Label = "";
        public ShapeTag Tag = new ShapeTag();
        // if state has return path to any previous state
        public bool ToPreviousState = false;
        public ShapeTag ToPreviousStateTag = new ShapeTag();
    }

    public class StateMethod
    {
        public string Id = Guid.NewGuid().ToString();
        public string Label = "";
        public ShapeTag Tag = new ShapeTag();
    }

    public class StateLink
    {
        public string FromState = "";
        public string FromAction = "";
        public string ToState = "";
        public List<string> BiDirectional = new List<string>();
        public ShapeTag Tag = new ShapeTag();
    }

    public class ShapeTag
    {
        public string FileName = "";
        public string JsonPath = "";
        //public string Label = "";
        public string Description = "";
        public Color Color = Color.Green;

        public override string ToString()
        {
            return $"File: {FileName}{Environment.NewLine}JSON path: {JsonPath}{Environment.NewLine}Description: {Description}";
        }
    }
}