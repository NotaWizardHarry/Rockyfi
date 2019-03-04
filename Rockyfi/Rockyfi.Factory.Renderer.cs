﻿using System;
using System.Collections.Generic;
using System.Xml;
using System.Text.RegularExpressions;
using System.IO;

namespace Rockyfi
{
    // MeasureFunc describes function for measuring
    public delegate Size MeasureFunc(Node node, float width, MeasureMode widthMode, float height, MeasureMode heightMode);

    // PrintFunc defines function for printing
    public delegate void PrintFunc(Node node);

    // BaselineFunc describes function for baseline
    public delegate float BaselineFunc(Node node, float width, float height);

    // Logger defines logging function
    public delegate int LoggerFunc(Config config, Node node, LogLevel level, string format, params object[] args);



    public partial class Node
    {
        internal void Helper_SetDimensions(Value value, Dimension dimension)
        {
            if (dimension == Dimension.Width)
            {
                if (value.unit == Unit.Auto)
                    StyleSetWidthAuto();
                else if (value.unit == Unit.Percent)
                    StyleSetWidthPercent(value.value);
                else if (value.unit == Unit.Point)
                    StyleSetWidth(value.value);
            }
            else
            {
                if (value.unit == Unit.Auto)
                    StyleSetHeightAuto();
                else if (value.unit == Unit.Percent)
                    StyleSetHeightPercent(value.value);
                else if (value.unit == Unit.Point)
                    StyleSetHeight(value.value);
            }
        }

        internal void Helper_SetMinDimensions(Value value, Dimension dimension)
        {
            if (dimension == Dimension.Width)
            {
                if (value.unit == Unit.Percent)
                    StyleSetMinWidthPercent(value.value);
                else if (value.unit == Unit.Point)
                    StyleSetMinWidth(value.value);
                else StyleSetMinWidth(float.NaN);
            }
            else
            {
                if (value.unit == Unit.Percent)
                    StyleSetMinHeightPercent(value.value);
                else if (value.unit == Unit.Point)
                    StyleSetMinHeight(value.value);
                else StyleSetMinHeight(float.NaN);
            }
        }

        internal void Helper_SetMaxDimensions(Value value, Dimension dimension)
        {
            if (dimension == Dimension.Width)
            {
                if (value.unit == Unit.Percent)
                    StyleSetMaxWidthPercent(value.value);
                else if (value.unit == Unit.Point)
                    StyleSetMaxWidth(value.value);
                else StyleSetMaxWidth(float.NaN);
            }
            else
            {
                if (value.unit == Unit.Percent)
                    StyleSetMaxHeightPercent(value.value);
                else if (value.unit == Unit.Point)
                    StyleSetMaxHeight(value.value);
                else StyleSetMaxHeight(float.NaN);
            }
        }

        internal void Helper_SetMarginPaddingBorder(string tag, Edge edge, Value value)
        {
            if (tag == "margin")
            {
                if (value.unit == Unit.Auto)
                    StyleSetMarginAuto(edge);
                else if (value.unit == Unit.Percent)
                    StyleSetMarginPercent(edge, value.value);
                else if (value.unit == Unit.Point)
                    StyleSetMargin(edge, value.value);
                else // if (value.unit == Unit.Undefined)
                    StyleSetMargin(edge, float.NaN);
            }
            else if (tag == "padding")
            {
                if (value.unit == Unit.Percent)
                    StyleSetPaddingPercent(edge, value.value);
                else if (value.unit == Unit.Point)
                    StyleSetPadding(edge, value.value);
                else StyleSetPadding(edge, float.NaN);
            }
            else if (tag == "border")
            {
                if (value.unit == Unit.Point)
                    StyleSetBorder(edge, value.value);
                else StyleSetBorder(edge, float.NaN);
            }
        }
    }

    public partial class Factory
    {
        const string ForELAttributeName = "el-for";
        const string IfELAttributeName = "el-if";
        const string BindELAttributePrefix = "el-bind";
        const string RootTagName = "div";
        const string ElementTagName = "div";

        static Regex styleValueRegex = new Regex(@"-?(\d*\.)?(\d+)(px|%)");

        ContextStack contextStack;
        XmlDocument xmlDocument;
        Node root;
        TemplateRendererNode templateRendererRoot;

        Value ParseValueFromString(string text)
        {
            if (text == "auto")
            {
                return new Value(0, Unit.Auto);
            }

            var res = Value.UndefinedValue;
            if (styleValueRegex.IsMatch(text))
            {
                string dig = text;
                Unit uu = Unit.Undefined;

                if (text.EndsWith("%"))
                {
                    dig = text.Substring(0, text.Length - 1);
                    uu = Unit.Percent;
                }
                else if (text.EndsWith("px"))
                {
                    dig = text.Substring(0, text.Length - 2);
                    uu = Unit.Point;
                }

                if (float.TryParse(dig, out res.value))
                {
                    res.unit = uu;
                }
            }

            return res;
        }

        /// <summary>
        /// margin="2px" | margin="12px 13px" | margin="12px 13px 1px" | margin="1px 2px 3px 4px"
        /// </summary>
        /// <param name="text"></param>
        /// <returns></returns>
        Value[] ParseFourValueFromString(string text)
        {
            // Edge.Left  = 0;
            // Edge.Top = 1;
            // Edge.Right = 2;
            // Edge.Bottom = 3;
            var vStr = text.Split(new char[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (vStr.Length > 4)
            {
                return null;
            }
            Value[] res = new Value[vStr.Length];
            for (int i = 0; i < res.Length; i++)
            {
                res[i] = ParseValueFromString(vStr[i]);
            }
            if (res.Length == 0)
            {
                return null;
            }
            if (res.Length == 1)
            {
                return new Value[] { res[0], res[0], res[0], res[0], };
            }
            else if (res.Length == 2)
            {
                return new Value[] { res[1], res[0], res[1], res[0], };
            }
            else if (res.Length == 3)
            {
                return new Value[] { res[1], res[0], res[1], res[2], };
            }
            if (res.Length > 4)
            {
                OnWarnning($"{text} too much params");
            }
            //else if (res.Length >= 4)
            return new Value[] { res[3], res[0], res[1], res[2], };
        }

        // margin-left --> ("margin", "left")
        // margin --> ("margin", "")
        static bool ParseBreakWork(string input, out string head, out string tail)
        {
            head = "";
            tail = "";
            var t = input.Split('-');
            if (t.Length == 2)
            {
                head = t[0];
                tail = t[1];
                return true;
            }
            if (t.Length == 1)
            {
                head = t[0];
                return true;
            }

            return false;
        }
        void OnWarnning(string msg)
        {

        }

        #region render virtual node to real node
        void ProcessNodeStyle(Node node, string attrKey, string attrValue)
        {
            switch (attrKey)
            {
                case "position":
                    if (Rockyfi.StringToPositionType(attrValue, out PositionType position))
                    {
                        node.StyleSetPositionType(position);
                    }
                    break;
                case "align-content":
                    if (Rockyfi.StringToAlign(attrValue, out Align alignContent))
                    {
                        node.StyleSetAlignContent(alignContent);
                    }
                    break;
                case "align-items":
                    if (Rockyfi.StringToAlign(attrValue, out Align alignItem))
                    {
                        node.StyleSetAlignItems(alignItem);
                    }
                    break;
                case "align-self":
                    if (Rockyfi.StringToAlign(attrValue, out Align alignSelf))
                    {
                        node.StyleSetAlignSelf(alignSelf);
                    }
                    break;
                case "flex-direction":
                    if (Rockyfi.StringToFlexDirection(attrValue, out FlexDirection flexDirection))
                    {
                        node.StyleSetFlexDirection(flexDirection);
                    }
                    break;
                case "flex-wrap":
                    if (Rockyfi.StringToWrap(attrValue, out Wrap flexWrap))
                    {
                        node.StyleSetFlexWrap(flexWrap);
                    }
                    break;
                case "flex-basis":
                    var flexBasisValue = ParseValueFromString(attrValue);
                    if (flexBasisValue.unit == Unit.Auto)
                    {
                        node.NodeStyleSetFlexBasisAuto();
                    }
                    else if (flexBasisValue.unit == Unit.Point)
                    {
                        node.StyleSetFlexBasis(flexBasisValue.value);
                    }
                    else if (flexBasisValue.unit == Unit.Percent)
                    {
                        node.StyleSetFlexBasisPercent(flexBasisValue.value);
                    }
                    break;
                case "flex-shrink":
                    if (float.TryParse(attrValue, out float flexShrink))
                    {
                        node.StyleSetFlexShrink(flexShrink);
                    }
                    break;
                case "flex-grow":
                    if (float.TryParse(attrValue, out float flexGrow))
                    {
                        node.StyleSetFlexGrow(flexGrow);
                    }
                    break;
                case "justify-content":
                    if (Rockyfi.StringToJustify(attrValue, out Justify justifyContent))
                    {
                        node.StyleSetJustifyContent(justifyContent);
                    }
                    break;
                case "direction":
                    if (Rockyfi.StringToDirection(attrValue, out Direction direction))
                    {
                        node.StyleSetDirection(direction);
                    }
                    break;
                case "width":
                    node.Helper_SetDimensions(ParseValueFromString(attrValue), Dimension.Width);
                    break;
                case "height":
                    node.Helper_SetDimensions(ParseValueFromString(attrValue), Dimension.Height);
                    break;
                case "min-width":
                    node.Helper_SetMinDimensions(ParseValueFromString(attrValue), Dimension.Width);
                    break;
                case "min-height":
                    node.Helper_SetMinDimensions(ParseValueFromString(attrValue), Dimension.Height);
                    break;
                case "max-width":
                    node.Helper_SetMaxDimensions(ParseValueFromString(attrValue), Dimension.Width);
                    break;
                case "max-height":
                    node.Helper_SetMaxDimensions(ParseValueFromString(attrValue), Dimension.Height);
                    break;
                default:
                    // parse [margin|padding|border]-[Edgexxxx]
                    if (ParseBreakWork(attrKey, out string head, out string tail))
                    {
                        if (head == "margin" || head == "padding" || head == "border")
                        {
                            if (tail == "")
                            {
                                var valueArray = ParseFourValueFromString(attrValue);
                                if (valueArray != null)
                                {
                                    for (int i = 0; i < valueArray.Length; i++)
                                    {
                                        node.Helper_SetMarginPaddingBorder(head, (Edge)i, valueArray[i]);
                                    }
                                }
                            }
                            else if (Rockyfi.StringToEdge(tail, out Edge edge))
                            {
                                node.Helper_SetMarginPaddingBorder(head, edge, ParseValueFromString(attrValue));
                            }
                        }
                    }
                    break;
            }
        }
        Node TemplateRendererNodeRender(TemplateRendererNode tnode, ContextStack contextStack, IEnumerable<object> forList)
        {
            Node node = Rockyfi.CreateDefaultNode();
            var ra = CreateRuntimeNodeAttribute(node, tnode);

            // set el-for value
            ra.forExpressCurrentValue = forList;

            // set style
            Style.Copy(node.nodeStyle, tnode.nodeStyle);

            // set el-bind binded style
            foreach (var attr in tnode.attributeDataBindExpressList)
            {
                if (attr.TryEvaluate(contextStack, out var attrValue))
                {
                    ProcessNodeStyle(node, attr.TargetName, attrValue != null ? attrValue.ToString() : "");
                    ra.attributes[attr.TargetKey] = attrValue;
                }
                else
                {
                    ra.attributes[attr.TargetKey] = null;
                }
            }

            // process innerText
            if (tnode.textDataBindExpress != null)
            {
                ra.textDataBindExpressCurrentValue = tnode.textDataBindExpress.Evaluate(contextStack);
            }

            // render children
            foreach (var vchild in tnode.Children)
            {
                foreach (var child in TemplateRendererNodeRenderToTree(vchild, contextStack))
                {
                    node.AddChild(child);
                }
            }

            return node;
        }
        LinkedList<Node> TemplateRendererNodeRenderToTree(TemplateRendererNode tnode, ContextStack contextStack)
        {
            LinkedList<Node> nodeList = new LinkedList<Node>();
            bool useFor = tnode.forExpress != null;
            IEnumerable<object> forList = useFor ? tnode.forExpress.Evaluate(contextStack) : new List<object> { 0 };
            foreach (object forContext in forList)
            {
                if (useFor)
                {
                    contextStack.EnterScope();
                    contextStack.Set(tnode.forExpress.IteratorName, forContext);
                }
                if (!(tnode.ifExpress != null
                    && tnode.ifExpress.TryEvaluate(contextStack, out var ifCondition)
                    && ifCondition == false))
                {
                    var node = TemplateRendererNodeRender(tnode, contextStack, useFor ? forList : null);
                    nodeList.AddLast(node);
                }
                if (useFor)
                {
                    contextStack.LeaveScope();
                }
            }

            return nodeList;
        }
        #endregion

        #region render xml to virtual node
        void ProcessTemplateStyle(Style style, string attrKey, string attrValue)
        {
            switch (attrKey)
            {
                case "direction":
                    if (Rockyfi.StringToDirection(attrValue, out Direction direction))
                    {
                        style.Direction = direction;
                    }
                    break;
                case "flex-direction":
                    if (Rockyfi.StringToFlexDirection(attrValue, out FlexDirection flexDirection))
                    {
                        style.FlexDirection = flexDirection;
                    }
                    break;
                case "justify-content":
                    if (Rockyfi.StringToJustify(attrValue, out Justify justifyContent))
                    {
                        style.JustifyContent = justifyContent;
                    }
                    break;
                case "align-content":
                    if (Rockyfi.StringToAlign(attrValue, out Align alignContent))
                    {
                        style.AlignContent = alignContent;
                    }
                    break;
                case "align-items":
                    if (Rockyfi.StringToAlign(attrValue, out Align alignItem))
                    {
                        style.AlignItems = alignItem;
                    }
                    break;
                case "align-self":
                    if (Rockyfi.StringToAlign(attrValue, out Align alignSelf))
                    {
                        style.AlignSelf = alignSelf;
                    }
                    break;
                case "flex-wrap":
                    if (Rockyfi.StringToWrap(attrValue, out Wrap flexWrap))
                    {
                        style.FlexWrap = flexWrap;
                    }
                    break;
                case "overflow":
                    if (Rockyfi.StringToOverflow(attrValue, out Overflow overflow))
                    {
                        style.Overflow = overflow;
                    }
                    break;
                case "display":
                    if (Rockyfi.StringToDisplay(attrValue, out Display display))
                    {
                        style.Display = display;
                    }
                    break;
                case "flex":
                    if (float.TryParse(attrValue, out float flex))
                    {
                        style.Flex = flex;
                    }
                    break;
                case "flex-grow":
                    if (float.TryParse(attrValue, out float flexGrow))
                    {
                        style.FlexGrow = flexGrow;
                    }
                    break;
                case "flex-shrink":
                    if (float.TryParse(attrValue, out float flexShrink))
                    {
                        style.FlexShrink = flexShrink;
                    }
                    break;
                case "flex-basis":
                    style.FlexBasis = ParseValueFromString(attrValue);
                    break;
                case "position":
                    if (Rockyfi.StringToPositionType(attrValue, out PositionType position))
                    {
                        style.PositionType = position;
                    }
                    break;
                case "width":
                    style.Dimensions[(int)Dimension.Width] = ParseValueFromString(attrValue);
                    break;
                case "height":
                    style.Dimensions[(int)Dimension.Height] = ParseValueFromString(attrValue);
                    break;
                case "min-width":
                    style.MinDimensions[(int)Dimension.Width] = ParseValueFromString(attrValue);
                    break;
                case "min-height":
                    style.MinDimensions[(int)Dimension.Height] = ParseValueFromString(attrValue);
                    break;
                case "max-width":
                    style.MaxDimensions[(int)Dimension.Width] = ParseValueFromString(attrValue);
                    break;
                case "max-height":
                    style.MaxDimensions[(int)Dimension.Height] = ParseValueFromString(attrValue);
                    break;
                default:
                    // parse [margin|padding|border]-[Edgexxxx]
                    if (ParseBreakWork(attrKey, out string head, out string tail))
                    {
                        Value[] valuesToSet = null;
                        switch (head)
                        {
                            case "margin": valuesToSet = style.Margin; break;
                            case "padding": valuesToSet = style.Padding; break;
                            case "border": valuesToSet = style.Border; break;
                        }
                        if (valuesToSet == null)
                            break;

                        if (tail == "")
                        {
                            var valueArray = ParseFourValueFromString(attrValue);
                            if (valueArray != null)
                            {
                                for (int i = 0; i < valueArray.Length; i++)
                                {
                                    valuesToSet[i] = valueArray[i];
                                }
                            }
                        }
                        else if (Rockyfi.StringToEdge(tail, out Edge edge))
                        {
                            valuesToSet[(int)edge] = ParseValueFromString(attrValue);
                        }
                    }
                    break;
            }
        }
        TemplateRendererNode ConvertXmlTreeToRendererTree(XmlNode element)
        {
            TemplateRendererNode renderTreeNode = new TemplateRendererNode();
            foreach (XmlAttribute attr in element.Attributes)
            {
                if (ForELAttributeName.Equals(attr.Name)) // process el-for
                {
                    renderTreeNode.forExpress = ForDataBindExpress.Parse(attr.Value);
                }
                else if (IfELAttributeName.Equals(attr.Name)) // process el-if
                {
                    renderTreeNode.ifExpress = IfDataBindExpress.Parse(attr.Value);
                }
                else if (BindELAttributePrefix.Equals(attr.Prefix)) // process el-bind:width="item.width"
                {
                    var attrExpress = AttributeDataBindExpress.Parse(attr.Value, attr.LocalName);
                    if (attrExpress != null)
                    {
                        renderTreeNode.attributeDataBindExpressList.AddLast(attrExpress);
                    }
                }
                else
                {
                    // try process style
                    ProcessTemplateStyle(renderTreeNode.nodeStyle, attr.Name, attr.Value);
                }

                renderTreeNode.attributes[attr.Name] = attr.Value;
            }

            // process children
            // render children
            foreach (XmlNode ele in element.ChildNodes)
            {
                if (XmlNodeType.Element == ele.NodeType)
                {
                    renderTreeNode.Children.Add(ConvertXmlTreeToRendererTree(ele));
                }
                else if (XmlNodeType.Text == ele.NodeType)
                {
                    renderTreeNode.textDataBindExpress = TextDataBindExpress.Parse(ele.Value);
                }
            }

            return renderTreeNode;
        }
        #endregion

        public Direction Direction = Direction.LTR;
        public float MaxWidth = float.NaN;
        public float MaxHeight = float.NaN;

        public void CalculateLayout()
        {
            root.CalculateLayout(MaxWidth, MaxHeight, Direction);
        }

        public delegate void DrawNodeFunc(float x, float y, float width, float height, string text, Dictionary<string, object> attribute);
        public void Draw(DrawNodeFunc drawFunc)
        {
            Queue<Node> queue = new Queue<Node>();
            queue.Enqueue(root);
            while (queue.Count != 0)
            {
                var node = queue.Dequeue();
                var contextAttr = GetNodeRuntimeAttribute(node);

                
                var nc = node.Context; // protected node.Context

                var attributes = new Dictionary<string, object>(contextAttr.attributes.Count 
                    + contextAttr.templateRendererNode.attributes.Count);

                foreach (var kv in contextAttr.templateRendererNode.attributes)
                    attributes[kv.Key] = kv.Value;
                foreach (var kv in contextAttr.attributes)
                    attributes[kv.Key] = kv.Value;

                drawFunc(node.LayoutGetLeft(), node.LayoutGetTop(),
                    node.LayoutGetWidth(), node.LayoutGetHeight(),
                    contextAttr.textDataBindExpressCurrentValue,
                    attributes // contextAttr.attributes ???
                    );
                if (!(nc == null && node.Context == null) && nc != node.Context)
                {
                    throw new Exception("cant change node.Context !");
                }
                node.Context = nc; // protected node.Context


                foreach (var child in node.Children)
                {
                    queue.Enqueue(child);
                }
            }
        }

        public void LoadFromString(string xml)
        {
            LoadFromString(xml, null);
        }

        public void LoadFromString(string xml, Dictionary<string, object> contextDictionary)
        {
            using (StringReader stringReader = new StringReader(xml))
            {
                XmlReaderSettings settings = new XmlReaderSettings { NameTable = new NameTable() };
                XmlNamespaceManager xmlns = new XmlNamespaceManager(settings.NameTable);
                xmlns.AddNamespace("el-bind", "Rockyfi.Factory");
                XmlParserContext context = new XmlParserContext(null, xmlns, "", XmlSpace.Default);
                XmlReader reader = XmlReader.Create(stringReader, settings, context);
                xmlDocument = new XmlDocument();
                xmlDocument.Load(reader);


                var rootElement = xmlDocument.FirstChild;

                //if (!RootTagName.Equals(rootElement.Name))
                //    throw new Exception("root element is not <div /> !");

                if (rootElement.Attributes.GetNamedItem(ForELAttributeName) != null)
                    throw new Exception("root element should not contains 'el-for' attribute !");

                if (rootElement.Attributes.GetNamedItem(ForELAttributeName) != null)
                    throw new Exception("root element should not contains 'el-if' attribute !");

                // convert to tree
                templateRendererRoot = ConvertXmlTreeToRendererTree(rootElement);

                // start render
                contextStack = new ContextStack(contextDictionary);
                root = TemplateRendererNodeRenderToTree(templateRendererRoot, contextStack).First.Value;

                //this.contextDictionary = contextDictionary;
                // Console.WriteLine(NodePrinter.PrintToString(root));
            }
        }

        public void SetData()
        {

        }
    }
}
