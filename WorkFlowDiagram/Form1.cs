// This is an independent project of an individual developer. Dear PVS-Studio, please check it.
// PVS-Studio Static Code Analyzer for C, C++, C#, and Java: http://www.viva64.com
using JsonEditorForm;

using JsonPathParserLib;

using Microsoft.Msagl.Core.Geometry.Curves;
using Microsoft.Msagl.Drawing;
using Microsoft.Msagl.GraphViewerGdi;

using Newtonsoft.Json;

using System.ComponentModel;
using System.Diagnostics;

using Color = Microsoft.Msagl.Drawing.Color;

namespace WorkFlowDiagram
{
    public partial class Form1 : Form
    {
        public struct WinPosition
        {
            public int WinX;
            public int WinY;
            public int WinW;
            public int WinH;

            [JsonIgnore] public bool Initialized => !(WinX <= 0 && WinY <= 0 && WinW <= 0 && WinH <= 0);
        }

        // configuration
        private const string PluginConfigFile = "appsettings.json";
        private Config<WorkFlowDiagramSettings> configBuilder = new();

        // diagram settings
        private const string _formCaption = "WorkFlow diagram";
        private const string _inputFileExtension = "*.json";
        private readonly Shape _shapeType = Shape.Box;
        private readonly ArrowStyle _arrowLineType = ArrowStyle.Normal;
        private readonly ArrowStyle _backArrowLineType = ArrowStyle.Diamond;
        private Color _startingShapeColor = Color.Green;
        private Color _endingShapeColor = Color.Yellow;
        private Color _orphanShapeColor = Color.Red;
        private Color _defaultStateColor = Color.LightBlue;
        private Color _defaultButtonColor = Color.WhiteSmoke;
        private Color _defaultActionColor = Color.Orange;
        private Color _returnOnlyShapeColor = Color.BlueViolet;

        // JSON parser settings
        private const string RootName = "";
        private const char PathDivider = '\\';
        private JsonPathParser _parser = new JsonPathParser();

        // Json viewer window
        private const string PreViewCaption = "JSON File ";
        private JsonViewer? _sideViewer;
        private bool _useVsCode = false;
        private readonly bool _singleLineBrackets = false;
        private WinPosition _editorPosition;
        private bool _showStateElements = true;
        private LayoutMethod _currentLayoutMode = LayoutMethod.SugiyamaScheme;
        private volatile bool _isSelectedTag = false;

        private readonly Dictionary<string, string> _actionAliases = new Dictionary<string, string>
        {
            { "add","add" },
            { "approvetask","approve" },
            { "approvenosave","approve" },
            { "completetask","complete" },
            { "declinetaskwithreason","decline" },
            { "declinetaskwithcomment","decline" },
            { "pausework","pausework" },
            { "reassigntask","reassigntask" },
            { "reassign","reassign" },
            { "suspendcertificate","suspendcertificate" },
            { "updateaudit","update"},
            { "updateandsendtoapproval","updateandsendtoapproval" },
            { "updateandsendtocancellation","updateandsendtocancellation" },
            { "updateandperformaction","updateandperformaction" },
            { "closetask","close" },
            { "destroydraft","destroydraft" },
            { "destroycertificate","destroycertificate" },
            { "updateandadd","add" },
            { "updatecertificate","update" }
        };

        private PlaneTransformation? _mouseDownTransform;
        private Point _mouseDownPoint;

        private Node? _currentState;

        #region GUI

        public Form1()
        {
            InitializeComponent();
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            this.Text = _formCaption;

            configBuilder = new Config<WorkFlowDiagramSettings>($"{PluginConfigFile}");
            if (!File.Exists($"{PluginConfigFile}"))
            {
                configBuilder.SaveConfig();
            }

            checkBox_useShelf.Checked = configBuilder.ConfigStorage.useShelf;

            CheckBox_UseShelf_CheckedChanged(this, EventArgs.Empty);

            checkBox_useVsCode.Checked = _useVsCode = configBuilder.ConfigStorage.UseVsCode;
            CheckBox_UseVsCode_CheckedChanged(this, EventArgs.Empty);

            checkBox_showStateElements.Checked = _showStateElements = configBuilder.ConfigStorage.ShowStateElements;
            CheckBox_showStateElements_CheckedChanged(this, EventArgs.Empty);

            _parser = new JsonPathParser
            {
                TrimComplexValues = false,
                SaveComplexValues = true,
                RootName = RootName,
                JsonPathDivider = PathDivider,
                SearchStartOnly = true
            };

            comboBox_layoutMode.Items.Add(Enum.GetName(LayoutMethod.SugiyamaScheme));
            comboBox_layoutMode.Items.Add(Enum.GetName(LayoutMethod.IcrementalLayout));
            comboBox_layoutMode.SelectedItem = _currentLayoutMode.ToString();

            string[] args = Environment.GetCommandLineArgs();
            if (args.Length == 2 && args[1].EndsWith(".msagl"))
            {
                LoadFile(args[1]);
                gViewer1.FileName = args[1];
                configBuilder.ConfigStorage.LastFile = args[1];
                this.Text = _formCaption + ": " + args[1];
            }
        }

        private void Button_LoadStates_Click(object sender, EventArgs e)
        {
            if (sender != null)
            {
                if (!string.IsNullOrEmpty(configBuilder.ConfigStorage.LastFolder))
                    folderBrowserDialog1.SelectedPath = configBuilder.ConfigStorage.LastFolder;

                if (folderBrowserDialog1.ShowDialog() != DialogResult.OK
                    || string.IsNullOrEmpty(folderBrowserDialog1.SelectedPath))
                    return;

                configBuilder.ConfigStorage.LastFolder = folderBrowserDialog1.SelectedPath;
            }
            else
            {
                folderBrowserDialog1.SelectedPath = configBuilder.ConfigStorage.LastFolder;
            }

            if (_showStateElements)
            {
                if (_currentLayoutMode.ToString() != Enum.GetName(LayoutMethod.SugiyamaScheme)
                    && _currentLayoutMode.ToString() != Enum.GetName(LayoutMethod.IcrementalLayout))
                {
                    comboBox_layoutMode.SelectedItem = Enum.GetName(LayoutMethod.SugiyamaScheme);
                }

                comboBox_layoutMode.Items.Remove(Enum.GetName(LayoutMethod.MDS));
                comboBox_layoutMode.Items.Remove(Enum.GetName(LayoutMethod.Ranking));
                comboBox_layoutMode.SelectedItem = _currentLayoutMode.ToString();
            }
            else
            {
                comboBox_layoutMode.Items.Add(Enum.GetName(LayoutMethod.MDS));
                comboBox_layoutMode.Items.Add(Enum.GetName(LayoutMethod.Ranking));
            }

            var filesList = Directory.GetFiles(folderBrowserDialog1.SelectedPath,
                _inputFileExtension,
                SearchOption.AllDirectories);

            LoadStates(filesList);

            this.Text = _formCaption + ": " + folderBrowserDialog1.SelectedPath;
        }

        private void CheckBox_UseShelf_CheckedChanged(object sender, EventArgs e)
        {
            configBuilder.ConfigStorage.useShelf = checkBox_useShelf.Checked;
            checkBox_useShelf.Text = checkBox_useShelf.Checked ? "SHELF json" : "NLMK json";
        }

        private void OnClosingEditor(object? sender, CancelEventArgs e)
        {
            if (sender is Form senderForm)
            {
                _editorPosition.WinX = senderForm.Location.X;
                _editorPosition.WinY = senderForm.Location.Y;
                _editorPosition.WinW = senderForm.Width;
                _editorPosition.WinH = senderForm.Height;
            }
        }

        private void OnResizeEditor(object? sender, EventArgs e)
        {
            if (sender is Form senderForm)
            {
                _editorPosition.WinX = senderForm.Location.X;
                _editorPosition.WinY = senderForm.Location.Y;
                _editorPosition.WinW = senderForm.Width;
                _editorPosition.WinH = senderForm.Height;
            }
        }

        private void CheckBox_UseVsCode_CheckedChanged(object sender, EventArgs e)
        {
            configBuilder.ConfigStorage.UseVsCode = _useVsCode = checkBox_useVsCode.Checked;
        }

        private void Form1_FormClosing(object sender, FormClosingEventArgs e)
        {
            configBuilder.SaveConfig();
        }

        private void GViewer1_ObjectUnderMouseCursorChanged(object sender, ObjectUnderMouseCursorChangedEventArgs e)
        {
            if (_isSelectedTag)
                return;

            var objectUnderCursor = gViewer1.ObjectUnderMouseCursor;
            if (objectUnderCursor == null)
            {
                textBox_tag.Clear();
            }
            else if (objectUnderCursor.DrawingObject is DrawingObject o && o.IsVisible)
            {
                var selectedObject = objectUnderCursor.DrawingObject;
                if (selectedObject is Edge edge)
                {
                    if (edge.UserData is ShapeTag tag)
                    {
                        textBox_tag.Text = tag.ToString();
                    }
                }
                else if (selectedObject is Node node && node.UserData is ShapeTag tag)
                {
                    textBox_tag.Text = tag.ToString();
                }
            }
        }

        private void GViewer1_DoubleClick(object sender, EventArgs e)
        {
            if (sender is Microsoft.Msagl.GraphViewerGdi.GViewer obj && obj.SelectedObject != null)
            {
                ShapeTag tag = new ShapeTag();

                if (obj.SelectedObject is Edge edge)
                {
                    if (edge.UserData is ShapeTag t)
                        tag = t;
                }
                else if (obj.SelectedObject is Node node && node.UserData is ShapeTag t)
                    tag = t;

                ShowPreviewEditor(tag.FileName, tag.JsonPath, true);
            }
        }

        private void ComboBox_layoutMode_SelectedIndexChanged(object sender, EventArgs e)
        {
            var newValue = comboBox_layoutMode.SelectedItem?.ToString();

            if (Enum.TryParse<LayoutMethod>(newValue, out var layoutMode))
            {
                _currentLayoutMode = layoutMode;
                gViewer1.CurrentLayoutMethod = _currentLayoutMode;
                gViewer1.Graph = gViewer1.Graph;
            }

            gViewer1.Focus();
        }

        private void CheckBox_showStateElements_CheckedChanged(object sender, EventArgs e)
        {
            configBuilder.ConfigStorage.ShowStateElements = _showStateElements = checkBox_showStateElements.Checked;
        }

        private void GViewer1_MouseDown(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Middle && ClientRectangle.Contains(PointToClient(MousePosition)))
            {
                _mouseDownPoint = new Point(e.X, e.Y);
                _mouseDownTransform = gViewer1.Transform.Clone();
            }
        }

        private void GViewer1_MouseMove(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Middle)
                ProcessPan(e);
        }

        private void ProcessPan(MouseEventArgs args)
        {
            if (ClientRectangle.Contains(args.X, args.Y))
            {
                if (_mouseDownTransform != null)
                {
                    gViewer1.Transform[0, 2] = _mouseDownTransform[0, 2] + args.X - _mouseDownPoint.X;
                    gViewer1.Transform[1, 2] = _mouseDownTransform[1, 2] + args.Y - _mouseDownPoint.Y;
                }
                gViewer1.Invalidate();
            }
        }

        private void GViewer1_MouseClick(object sender, MouseEventArgs e)
        {
            if (e.Button == System.Windows.Forms.MouseButtons.Left)
            {

                var objectUnderCursor = gViewer1?.ObjectUnderMouseCursor;
                if (objectUnderCursor == null)
                {
                    _isSelectedTag = false;
                    textBox_tag.Clear();
                }
                else if (objectUnderCursor.DrawingObject is DrawingObject o && o.IsVisible)
                {
                    _isSelectedTag = true;
                    var selectedObject = objectUnderCursor.DrawingObject;
                    if (selectedObject is Edge edge)
                    {
                        if (edge.UserData is ShapeTag tag)
                        {
                            textBox_tag.Text = tag.ToString();
                        }
                    }
                    else if (selectedObject is Node node && node.UserData is ShapeTag tag)
                    {
                        textBox_tag.Text = tag.ToString();
                    }
                }
            }
        }

        private void HideLinksToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HideLinks(gViewer1.Graph, _currentState);
        }

        private void HideStateToolStripMenuItem_Click(object sender, EventArgs e)
        {
            HideState(gViewer1.Graph, _currentState, true);
        }

        private void UnhideAllToolStripMenuItem_Click(object sender, EventArgs e)
        {
            UnHideAll(gViewer1.Graph);
        }

        private void ContextMenuStrip1_Opening(object sender, CancelEventArgs e)
        {
            var objectUnderCursor = gViewer1?.ObjectUnderMouseCursor?.DrawingObject;
            if (objectUnderCursor is Subgraph state && state.IsVisible)
            {
                _currentState = state;
                contextMenuStrip1.Items[0].Visible = true;
                contextMenuStrip1.Items[1].Visible = true;
            }
            else
            {
                contextMenuStrip1.Items[0].Visible = false;
                contextMenuStrip1.Items[1].Visible = false;
            }
        }

        #endregion GUI

        #region Utilities

        private void LoadFile(string fullFileName)
        {
            gViewer1.NeedToCalculateLayout = false;
            gViewer1.Graph = Graph.Read(fullFileName);
            gViewer1.FileName = fullFileName;
            gViewer1.NeedToCalculateLayout = true;
        }

        private void LoadStates(IEnumerable<string> filesList)
        {
            var states = new List<State>();
            var links = new List<StateLink>();
            foreach (var file in filesList)
            {
                var newPathList = ParseJson(file, _parser);

                if (newPathList == null)
                    continue;

                State? newState;
                List<StateLink> newLinks;

                if (configBuilder.ConfigStorage.useShelf)
                    GetStateShelf(newPathList, file, out newState, out newLinks);
                else
                    GetStateNlmk(newPathList, file, out newState, out newLinks);

                if (newState != null)
                {
                    states.Add(newState);

                    if (newLinks != null && newLinks.Any())
                    {
                        links.AddRange(newLinks);
                    }
                }
            }

            var flowName = new DirectoryInfo(folderBrowserDialog1.SelectedPath).Name;
            var graph = InitProject(flowName);

            FixStateLinks(ref links, states);
            states.AddRange(FindOrphanStates(states, links));
            FixStateLinks(ref links, states);
            FindBiDirectionalLinks(states, ref links);
            FindStartingStates(ref states, links);
            FindEndingStates(ref states, links);

            CreateStates(graph, states);
            CreateLinks(graph, links);

            gViewer1.Focus();

            RefreshLayout(graph);
        }

        private Graph InitProject(string currentDiagramName)
        {
            this.Text = currentDiagramName;
            gViewer1.Graph = new Graph();
            gViewer1.CurrentLayoutMethod = _currentLayoutMode;
            var graph = new Graph(currentDiagramName);

            return graph;
        }

        private void GetStateShelf(IEnumerable<ParsedProperty> pathList, string filePath, out State? state, out List<StateLink> stateLinks)
        {
            state = null;
            stateLinks = new List<StateLink>();

            // find state defined
            var stateObject = pathList.FirstOrDefault(n => n.Path.Equals(RootName + PathDivider + "state", StringComparison.OrdinalIgnoreCase));

            if (stateObject == null)
                return;

            var stateDescription = pathList.FirstOrDefault(n => n.Name == "description" && n.ParentPath.Equals(stateObject.ParentPath, StringComparison.OrdinalIgnoreCase))?.Value ?? "";

            // create new state
            state = new State()
            {
                Label = stateObject.Value,
                Tag = new ShapeTag
                {
                    Description = stateDescription,
                    FileName = filePath,
                    JsonPath = stateObject.Path,
                    Color = _defaultStateColor
                }
            };

            // find all buttons
            var buttons = pathList
                .Where(n =>
                    n.Name.Equals("data", StringComparison.OrdinalIgnoreCase)
                    && n.ParentPath.StartsWith(RootName + PathDivider + "buttons", StringComparison.OrdinalIgnoreCase))
                .ToList();

            foreach (var button in buttons)
            {
                var buttonName = pathList.FirstOrDefault(n => n.Name.Equals("caption", StringComparison.OrdinalIgnoreCase) && n.ParentPath.Equals(button.Path, StringComparison.OrdinalIgnoreCase));
                var methodsNames = pathList
                    .Where(n => (n.Name.Equals("proceed", StringComparison.OrdinalIgnoreCase)
                            || n.Name.Equals("complete", StringComparison.OrdinalIgnoreCase)
                            || n.Name.Equals("actionToPerform", StringComparison.OrdinalIgnoreCase))
                        && n.ParentPath.Equals(button.Path, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var internalButtons = FindInternalButtonsRecursive(pathList, filePath, button);

                var newButton = new StateButton()
                {
                    Methods = methodsNames.Select(n => new StateMethod()
                    {
                        Label = n.Value,
                        Tag = new ShapeTag()
                        {
                            Description = n.Value,
                            JsonPath = n.Path,
                            FileName = filePath
                        }
                    }).ToList(),
                    InternalButtons = internalButtons,
                    Label = buttonName?.Value ?? "",
                    Tag = new ShapeTag()
                    {
                        Description = buttonName?.Value ?? "",
                        FileName = filePath,
                        JsonPath = buttonName?.Path ?? "",
                        Color = _defaultButtonColor
                    }
                };

                state.StateButtons.Add(newButton);
            }

            // find all transitions defined
            var transitions = pathList
                .Where(n =>
                    n.Name.Equals("state", StringComparison.OrdinalIgnoreCase)
                    && n.ParentPath.Contains(PathDivider + "transition"))
                .ToList();

            // generate links from current state
            if (transitions.Any())
            {
                foreach (var transition in transitions)
                {
                    // get the name of the method which is running the transition for description
                    var actionName = pathList.FirstOrDefault(n => n.Name.Equals("name", StringComparison.OrdinalIgnoreCase) && n.ParentPath == TrimPathEnd(transition.ParentPath, 1, PathDivider));

                    if (actionName == null)
                    {
                        actionName = pathList.FirstOrDefault(n => n.Name.Equals("entry", StringComparison.OrdinalIgnoreCase) && n.ParentPath == TrimPathEnd(transition.ParentPath, 2, PathDivider));
                        if (actionName == null)
                        {
                            continue;
                        }
                    }

                    var newAction = new StateAction()
                    {
                        Label = actionName.JsonPropertyType == JsonPropertyType.Property ? actionName.Value : actionName.Name,
                        Tag = new ShapeTag
                        {
                            FileName = filePath,
                            JsonPath = actionName.Path,
                            Color = _defaultActionColor
                        },
                    };
                    state.StateActions.Add(newAction);

                    var newLink = new StateLink()
                    {
                        FromState = state.Id,
                        FromAction = newAction.Id,
                        ToState = transition.Value,
                        Tag = new ShapeTag
                        {
                            Description = state.Label + "->" + transition.Value,
                            FileName = filePath,
                            JsonPath = transition.Path,
                        },
                    };
                    stateLinks.Add(newLink);
                }
            }

            // find all calculated transitions defined and create links for them
            var ctransitions = pathList.Where(n =>
                n.Name.Equals("calculatedState", StringComparison.OrdinalIgnoreCase)
                && n.ParentPath.Contains(PathDivider + "transition"))
                .ToList();
            var nextStateTransitions = ctransitions.Where(n => n.Value == "NextStateByInitialRiskValue");
            if (nextStateTransitions.Any())
            {
                foreach (var cState in nextStateTransitions)
                {
                    var calcutatedTransitions = pathList.Where(n => n.ParentPath.Equals(cState.ParentPath + PathDivider + "calculatedStateParams", StringComparison.OrdinalIgnoreCase));

                    foreach (var transition in calcutatedTransitions)
                    {
                        // get the name of the method which is running the transition for description
                        var actionName = pathList
                            .FirstOrDefault(n => n.Name.Equals("name", StringComparison.OrdinalIgnoreCase)
                                && n.ParentPath.Equals(TrimPathEnd(transition.ParentPath, 2, PathDivider), StringComparison.OrdinalIgnoreCase));

                        if (actionName == null)
                        {
                            continue;
                        }

                        var newAction = new StateAction()
                        {
                            Label = actionName.JsonPropertyType == JsonPropertyType.Property ? actionName.Value : actionName.Name,
                            Tag = new ShapeTag
                            {
                                FileName = filePath,
                                JsonPath = actionName.Path,
                                Color = _defaultActionColor
                            },
                        };
                        state.StateActions.Add(newAction);

                        var newLink = new StateLink()
                        {
                            FromState = state.Id,
                            FromAction = newAction.Id,
                            ToState = transition.Name,
                            Tag = new ShapeTag
                            {
                                Description = state.Label + "->" + transition.Name,
                                FileName = filePath,
                                JsonPath = transition.Path,
                            }
                        };
                        stateLinks.Add(newLink);
                    }
                }
            }

            // find all PreviousState transitions defined
            var returnLinks = ctransitions.Where(n => n.Value.Equals("GetPreviousState", StringComparison.OrdinalIgnoreCase));
            if (returnLinks.Any())
            {
                foreach (var returnLink in returnLinks)
                {
                    var actionName = pathList.FirstOrDefault(n => n.Name.Equals("name", StringComparison.OrdinalIgnoreCase) && n.ParentPath.Equals(TrimPathEnd(returnLink.ParentPath, 1, PathDivider), StringComparison.OrdinalIgnoreCase));

                    var newReturnTag = new ShapeTag()
                    {
                        Description = returnLink.Value,
                        FileName = filePath,
                        JsonPath = returnLink.Path
                    };

                    var stateActions = new StateAction()
                    {
                        Label = actionName?.Value ?? "",
                        Tag = new ShapeTag
                        {
                            FileName = filePath,
                            JsonPath = actionName?.Path ?? "",
                            Color = _returnOnlyShapeColor
                        },
                        ToPreviousState = true,
                        ToPreviousStateTag = newReturnTag
                    };
                    state.StateActions.Add(stateActions);
                }
            }
        }

        private List<StateButton> FindInternalButtonsRecursive(IEnumerable<ParsedProperty> pathList, string filePath, ParsedProperty startProperty)
        {
            // find all internal buttons
            var intButtons = pathList
                .Where(n =>
                    n.Name.Equals("buttons", StringComparison.OrdinalIgnoreCase)
                    && n.Path.StartsWith(startProperty.Path, StringComparison.OrdinalIgnoreCase)
                    && n.Depth > startProperty.Depth + 1)
                .ToList();

            var intenalButtons = new List<StateButton>();
            foreach (var buttonArray in intButtons)
            {
                var buttons = pathList.Where(n => n.Path.StartsWith(buttonArray.Path, StringComparison.OrdinalIgnoreCase) && n.Path.EndsWith(PathDivider + "caption", StringComparison.OrdinalIgnoreCase) && n.Depth == buttonArray.Depth + 2);
                foreach (var button in buttons)
                {
                    var methodName = pathList.FirstOrDefault(n => n.ParentPath.Equals(button.ParentPath, StringComparison.OrdinalIgnoreCase) && n.Name.Equals("action", StringComparison.OrdinalIgnoreCase));
                    var internalButtons = FindInternalButtonsRecursive(pathList, filePath, button);

                    var newButton = new StateButton()
                    {
                        InternalButtons = internalButtons,
                        Label = button.Value,
                        Tag = new ShapeTag()
                        {
                            Description = button.Value,
                            FileName = filePath,
                            JsonPath = button.Path,
                            Color = _defaultButtonColor
                        }
                    };

                    newButton.Methods.Add(new StateMethod()
                    {
                        Label = methodName?.Value ?? "",
                        Tag = new ShapeTag()
                        {
                            Description = methodName?.Value ?? "",
                            FileName = filePath,
                            JsonPath = methodName?.Path ?? ""
                        }
                    });

                    intenalButtons.Add(newButton);
                }
            }

            return intenalButtons;
        }

        private void GetStateNlmk(IEnumerable<ParsedProperty> pathList, string filePath, out State? state, out List<StateLink> stateLinks)
        {
            state = null;
            stateLinks = new List<StateLink>();

            // ignore metadata file
            if (filePath.EndsWith("configuration.json", StringComparison.OrdinalIgnoreCase))
                return;

            // find state defined
            var stateObject = pathList.FirstOrDefault(n => n.Path.Equals(RootName + PathDivider + "name", StringComparison.OrdinalIgnoreCase));
            var stateDescription = pathList.FirstOrDefault(n => n.Name == "description" && n.ParentPath.Equals(stateObject?.ParentPath, StringComparison.OrdinalIgnoreCase))?.Value ?? "";

            if (stateObject == null)
                return;

            state = new State()
            {
                Label = stateObject.Value,
                Tag = new ShapeTag
                {
                    Description = stateDescription,
                    FileName = filePath,
                    JsonPath = stateObject.Path,
                    Color = _defaultStateColor
                }
            };

            // find all buttons
            var buttons = pathList.Where(n => n.Name.Equals("title", StringComparison.OrdinalIgnoreCase) && n.ParentPath.StartsWith(RootName + PathDivider + "data" + PathDivider + "buttons", StringComparison.OrdinalIgnoreCase)).ToList();
            foreach (var button in buttons)
            {
                var methodsNames = pathList
                    .Where(n =>
                        n.Name.Equals("action", StringComparison.OrdinalIgnoreCase)
                        && n.ParentPath.Equals(button.ParentPath, StringComparison.OrdinalIgnoreCase)
                        && !string.IsNullOrEmpty(n.Value))
                    .ToList();

                var newButton = new StateButton()
                {
                    Methods = methodsNames.Select(n => new StateMethod()
                    {
                        Label = n.Value,
                        Tag = new ShapeTag()
                        {
                            Description = n.Value,
                            FileName = filePath,
                            JsonPath = n.Path
                        }
                    }).ToList(),
                    Label = button.Value,
                    Tag = new ShapeTag()
                    {
                        Description = button.Value,
                        FileName = filePath,
                        JsonPath = button.Path,
                        Color = _defaultButtonColor
                    }
                };

                state.StateButtons.Add(newButton);
            }

            // find all transitions defined
            var transitions = pathList.Where(n => n.Name.Equals("type", StringComparison.OrdinalIgnoreCase) && n.Value.Equals("transition", StringComparison.OrdinalIgnoreCase)).ToList();

            // Generate links from current state
            // state_name_to, filter_method_name
            foreach (var transitionPath in transitions.Select(n => n.ParentPath))
            {
                var actionName = pathList.FirstOrDefault(n => n.Path == transitionPath);
                var transitionName = pathList.FirstOrDefault(n => n.Name.Equals("name", StringComparison.OrdinalIgnoreCase) && n.ParentPath == transitionPath);

                var actionNameStr = actionName?.Name ?? "";
                if (actionName != null && string.IsNullOrEmpty(actionNameStr))
                {
                    var i = actionName.Path?.LastIndexOf(PathDivider) ?? 0;
                    if (i > 1)
                    {
                        actionNameStr = actionName.Path?.Substring(i + 1) ?? "";
                        i = actionNameStr.IndexOf('[');

                        if (i > 1)
                            actionNameStr = actionNameStr.Substring(0, i);
                    }
                }

                var newAction = new StateAction()
                {
                    Label = actionNameStr,
                    Tag = new ShapeTag()
                    {
                        FileName = filePath,
                        JsonPath = actionName?.Path ?? "",
                        Color = _defaultActionColor
                    }
                };
                state.StateActions.Add(newAction);

                var newLink = new StateLink()
                {
                    FromState = state.Id,
                    FromAction = newAction.Id,
                    ToState = transitionName?.Value ?? "",
                    Tag = new ShapeTag
                    {
                        Description = stateObject.Value + "->" + transitionName?.Value,
                        FileName = filePath,
                        JsonPath = transitionName?.Path ?? ""
                    }
                };

                stateLinks.Add(newLink);
            }
        }

        private static IEnumerable<ParsedProperty> ParseJson(string filePath, JsonPathParser parser)
        {
            string text;
            IEnumerable<ParsedProperty> newPathList = new List<ParsedProperty>();
            try
            {
                text = File.ReadAllText(filePath);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
                return newPathList;
            }

            // replace &nbsp char with space
            if (text.Contains((char)160))
            {
                text = text.Replace((char)160, (char)32);
            }

            try
            {
                newPathList = parser.ParseJsonToPathList(text, out var pos, out var errorFound);
                // parsing failed
                if (errorFound)
                {
                    MessageBox.Show($"Error parsing \"{filePath}\" at position {pos}");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            return newPathList;
        }

        private void CreateStates(Graph graph, List<State> states)
        {
            foreach (var state in states)
            {
                if (_showStateElements)
                {
                    var stateGroup = BuildSubShape(state);
                    graph.RootSubgraph.AddSubgraph(stateGroup);

                    // create actions
                    foreach (var action in state.StateActions)
                    {
                        var stateAction = BuildShape(action);
                        stateGroup.AddNode(stateAction);
                        graph.AddNode(stateAction);
                    }

                    // create buttons
                    CreateButtonsRecursive(graph, stateGroup, state, null, state.StateButtons);
                }
                else
                {
                    var stateNode = BuildShape(state);
                    graph.AddNode(stateNode);
                }
            }
        }

        private void CreateButtonsRecursive(Graph graph, Subgraph stateGroup, State state, StateButton? rootButton, List<StateButton> buttons)
        {
            foreach (var button in buttons)
            {
                if (string.IsNullOrEmpty(button.Label) && !button.Methods.Any())
                    continue;

                var stateButton = BuildShape(button);
                stateGroup.AddNode(stateButton);
                graph.AddNode(stateButton);

                // link root button to button
                if (rootButton != null) DrawLink(graph, rootButton.Id, button.Id, rootButton.Tag);

                // link buttons with actions
                if (configBuilder.ConfigStorage.useShelf)
                {
                    foreach (var buttonMethod in button.Methods)
                    {
                        if (_actionAliases.TryGetValue(buttonMethod?.Label?.ToLowerInvariant() ?? "", out var stateAction)
                            && state.StateActions.Any(n => n.Label.Equals(stateAction, StringComparison.OrdinalIgnoreCase)))
                        {
                            var toNames = state.StateActions.Where(n => n.Label.Equals(stateAction, StringComparison.OrdinalIgnoreCase));
                            foreach (var toName in toNames)
                            {
                                DrawLink(graph, button.Id, toName.Id, button.Tag);
                            }
                        }
                        else if (!string.IsNullOrEmpty(buttonMethod?.Label))
                        {
                            // create unrecognized action
                            buttonMethod.Tag.Color = Color.Red;
                            var action = BuildShape(new StateAction()
                            {
                                Label = buttonMethod.Label + "()",
                                Tag = buttonMethod.Tag
                            });
                            stateGroup.AddNode(action);
                            graph.AddNode(action);
                            DrawLink(graph, button.Id, action.Id, button.Tag);
                        }
                    }
                }
                else
                {
                    foreach (var buttonMethod in button.Methods)
                    {
                        var toName = state.StateActions.FirstOrDefault(n => n.Label.Equals(buttonMethod.Label, StringComparison.OrdinalIgnoreCase));
                        if (toName != null && !string.IsNullOrEmpty(toName.Id))
                        {
                            DrawLink(graph, button.Id, toName.Id, button.Tag);
                        }
                    }
                }

                CreateButtonsRecursive(graph, stateGroup, state, button, button.InternalButtons);
            }
        }

        private Node BuildShape(StateAction action)
        {
            var shape = new Node(action.Id)
            {
                UserData = action.Tag
            };

            shape.LabelText = action.Label;
            shape.Attr.FillColor = action.Tag.Color;
            shape.Attr.Shape = _shapeType;

            return shape;
        }

        private Node BuildShape(StateButton button)
        {
            var shape = new Node(button.Id)
            {
                UserData = button.Tag
            };

            shape.LabelText = button.Label;
            shape.Attr.FillColor = button.Tag.Color;
            shape.Attr.Shape = _shapeType;

            return shape;
        }

        private Node BuildShape(State state)
        {
            var shape = new Node(state.Id)
            {
                UserData = state.Tag
            };

            shape.LabelText = state.Label;
            shape.Attr.FillColor = state.Tag.Color;
            shape.Attr.Shape = _shapeType;

            return shape;
        }

        private Subgraph BuildSubShape(State state)
        {
            var shape = new Subgraph(state.Id)
            {
                UserData = state.Tag,
                LabelText = state.Label + Environment.NewLine + state.Tag.Description
            };
            shape.Attr.FillColor = state.Tag.Color;
            shape.Attr.Shape = _shapeType;

            return shape;
        }

        private void CreateLinks(Graph graph, List<StateLink> links)
        {
            foreach (var link in links)
            {
                if (_showStateElements)
                    DrawLink(graph, link.FromAction, link.ToState, link.Tag, link.BiDirectional.Any());
                else
                    DrawLink(graph, link.FromState, link.ToState, link.Tag, link.BiDirectional.Any());
            }
        }

        private void DrawLink(Graph graph, string fromName, string toName, ShapeTag tag, bool biDirectional = false)
        {
            var arrow = graph.AddEdge(fromName, toName);
            arrow.UserData = tag;
            arrow.Attr.ArrowheadAtTarget = _arrowLineType;
            arrow.Attr.ArrowheadLength = 30;

            if (biDirectional)
            {
                arrow.Attr.ArrowheadAtTarget = _backArrowLineType;
                arrow.Attr.ArrowheadAtSource = _backArrowLineType;
            }
            else
            {
                arrow.Attr.ArrowheadAtTarget = _arrowLineType;
            }
        }

        private static void FixStateLinks(ref List<StateLink> links, List<State> states)
        {
            foreach (var link in links)
            {
                var toStateId = states.FirstOrDefault(n => n.Label == link.ToState)?.Id;

                if (!string.IsNullOrEmpty(toStateId))
                    link.ToState = toStateId;
            }
        }

        private List<State> FindOrphanStates(List<State> states, List<StateLink> links)
        {
            var statesWithInput = links.Select(n => n.ToState).Distinct().ToList();
            var statesWithOutput = links.Select(n => n.FromState).Distinct().ToList();
            var allLinkedStates = statesWithInput.Concat(statesWithOutput).Distinct().ToList();

            var orphanStates = new List<State>();
            // �������� � ���������� ������� ������ ��� ����������� (�� ������������)
            var allStates = states.Select(n => n.Id).Distinct().ToList();
            var noLinkStates = allLinkedStates.Except(allStates).ToList();
            foreach (var state in noLinkStates)
            {
                var linksToFalseState = links.Where(n => n.ToState == state || n.FromAction == state);

                var newState = new State()
                {
                    Orphan = true,
                    Label = state,
                    Tag = new ShapeTag
                    {
                        FileName = linksToFalseState.FirstOrDefault()?.Tag?.FileName ?? string.Empty,
                        JsonPath = linksToFalseState.FirstOrDefault()?.Tag?.JsonPath ?? string.Empty,
                        Color = _orphanShapeColor
                    }
                };

                orphanStates.Add(newState);
            }

            return orphanStates;
        }

        private static void FindBiDirectionalLinks(List<State> states, ref List<StateLink> links)
        {
            foreach (var link in links)
            {
                // there should be exactly one state to point to
                var remoteState = states.FirstOrDefault(n => n.Id == link.ToState);

                // create reverse links to all links defined in the remote state
                if (remoteState != null)
                {
                    var actions = remoteState.StateActions.Where(n => n.ToPreviousState)?.Select(n => n.Id)?.ToList();

                    if (actions != null && actions.Any())
                        link.BiDirectional = actions;
                }
            }
        }

        private void FindStartingStates(ref List<State> states, List<StateLink> links)
        {
            var allStates = states.Where(n => !n.Orphan).Select(n => n.Id).ToList();
            var statesWithInput = links.Select(n => n.ToState).ToList();
            var t = links.Where(n => n.BiDirectional.Any()).Select(n => n.FromState).Distinct();
            statesWithInput.AddRange(t);
            statesWithInput = statesWithInput.Distinct().ToList();

            // ���������� ������� �����, �� ������� ���� ������ ������ (���������)
            var startStateNames = allStates.Except(statesWithInput).ToList();
            foreach (var startName in startStateNames)
            {
                var startState = states.FirstOrDefault(n => n.Id == startName);
                if (startState == null)
                    continue;

                startState.Tag.Color = _startingShapeColor;
            }
        }

        private void FindEndingStates(ref List<State> states, List<StateLink> links)
        {
            var allStates = states.Where(n => !n.Orphan).Select(n => n.Id).ToList();
            var statesWithOutput = links.Select(n => n.FromState).Distinct().ToList();
            var t = links.SelectMany(n => n.BiDirectional).Distinct();
            statesWithOutput.AddRange(t);
            statesWithOutput = statesWithOutput.Distinct().ToList();

            // ���������� ������ ������, � ������� ���� ������ ����� (��������)
            var endStateNames = allStates.Except(statesWithOutput).ToList();
            foreach (var stateName in endStateNames)
            {
                var endState = states.FirstOrDefault(n => n.Id == stateName && !n.StateActions.Any(m => m.ToPreviousState));
                if (endState != null)
                    endState.Tag.Color = _endingShapeColor;
            }
        }

        private void RefreshLayout(Graph graph)
        {
            if (graph == null || graph.Nodes == null)
                return;

            gViewer1.Graph = graph;
        }

        private void ShowPreviewEditor(string longFileName,
            string jsonPath,
            bool standAloneEditor = false)
        {
            if (_useVsCode)
            {
                var lineNumber = GetLineNumberForPath(longFileName, jsonPath);
                var execParams = "-r -g " + longFileName + ":" + lineNumber;
                VsCodeOpenFile(execParams);

                return;
            }

            var textEditor = _sideViewer;
            if (standAloneEditor) textEditor = null;

            var fileLoaded = false;
            var newWindow = false;
            if (textEditor != null && !textEditor.IsDisposed)
            {
                if (textEditor.SingleLineBrackets != _singleLineBrackets ||
                    textEditor.Text != PreViewCaption + longFileName)
                {
                    textEditor.SingleLineBrackets = _singleLineBrackets;
                    fileLoaded = textEditor.LoadJsonFromFile(longFileName);
                }
                else
                {
                    fileLoaded = true;
                }
            }
            else
            {
                if (textEditor != null)
                {
                    textEditor.Close();
                    textEditor.Dispose();
                }

                textEditor = new JsonViewer("", "", standAloneEditor)
                {
                    SingleLineBrackets = _singleLineBrackets
                };

                newWindow = true;
                fileLoaded = textEditor.LoadJsonFromFile(longFileName);
            }

            if (!standAloneEditor)
                _sideViewer = textEditor;

            textEditor.AlwaysOnTop = false;
            textEditor.Show();

            if (!standAloneEditor && newWindow)
            {
                if (!(_editorPosition.WinX == 0
                      && _editorPosition.WinY == 0
                      && _editorPosition.WinW == 0
                      && _editorPosition.WinH == 0))
                {
                    textEditor.Location = new System.Drawing.Point(_editorPosition.WinX, _editorPosition.WinY);
                    textEditor.Width = _editorPosition.WinW;
                    textEditor.Height = _editorPosition.WinH;
                }

                textEditor.Closing += OnClosingEditor;
                textEditor.ResizeEnd += OnResizeEditor;
            }

            if (!fileLoaded)
            {
                textEditor.Text = "Failed to load " + longFileName;
                return;
            }

            if (!standAloneEditor)
                textEditor.Text = PreViewCaption + longFileName;
            else
                textEditor.Text = longFileName;

            textEditor.HighlightPathJson(jsonPath, PathDivider);
        }

        private static void VsCodeOpenFile(string command)
        {
            var processInfo = new ProcessStartInfo("code", command)
            {
                CreateNoWindow = true,
                UseShellExecute = true,
                WindowStyle = ProcessWindowStyle.Hidden
            };

            try
            {
                Process.Start(processInfo);
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }

        private int GetLineNumberForPath(string longFileName, string jsonPath)
        {
            string jsonStr;
            try
            {
                jsonStr = File.ReadAllText(longFileName);
            }
            catch
            {
                return 0;
            }

            if (string.IsNullOrEmpty(jsonStr))
                return 0;

            var startLine = 0;
            var property = _parser.SearchJsonPath(jsonStr, jsonPath);
            _parser.SearchStartOnly = true;

            if (property != null)
            {
                JsonPathParser.GetLinesNumber(jsonStr, property.StartPosition, property.EndPosition, out startLine,
                    out var _);
            }

            return startLine;
        }

        private static string TrimPathEnd(string originalPath, int levels, char pathDivider)
        {
            for (; levels > 0; levels--)
            {
                var pos = originalPath.LastIndexOf(pathDivider);
                if (pos >= 0)
                {
                    originalPath = originalPath.Substring(0, pos);
                }
                else
                    break;
            }
            return originalPath;
        }

        static void HideState(Graph graph, Node state, bool removeLinks)
        {
            if (state == null)
                return;

            state.IsVisible = false;

            if (state is Subgraph sg)
            {
                foreach (var node in sg.Nodes)
                {
                    node.IsVisible = false;
                }
            }

            if (removeLinks)
            {
                HideLinks(graph, state);
            }
        }

        static void HideLinks(Graph graph, Node state)
        {
            if (state == null)
                return;

            foreach (var edge in graph.Edges.Where(n => n.SourceNode == state || n.TargetNode == state))
            {
                edge.IsVisible = false;
            }
        }

        static void UnHideAll(Graph graph)
        {
            foreach (var s in graph.SubgraphMap)
            {
                s.Value.IsVisible = true;
            }

            foreach (var node in graph.Nodes)
            {
                node.IsVisible = true;
            }

            foreach (var edge in graph.Edges)
            {
                edge.IsVisible = true;
            }
        }

        #endregion Utilities

        private void GViewer1_GraphSavingEnded(object sender, EventArgs e)
        {
            if (gViewer1?.FileName?.EndsWith(".msagl") ?? false)
                this.Text = _formCaption + ": " + gViewer1.FileName;
        }

        private void GViewer1_CustomOpenButtonPressed(object sender, HandledEventArgs e)
        {
            openFileDialog1.FileName = string.IsNullOrEmpty(gViewer1.FileName) ? gViewer1.FileName : configBuilder.ConfigStorage.LastFile;

            if (openFileDialog1.ShowDialog() != DialogResult.OK
                || string.IsNullOrEmpty(openFileDialog1.FileName))
                return;

            LoadFile(openFileDialog1.FileName);
            configBuilder.ConfigStorage.LastFile = openFileDialog1.FileName;
            this.Text = _formCaption + ": " + openFileDialog1.FileName;
            e.Handled = true;
        }
    }
}