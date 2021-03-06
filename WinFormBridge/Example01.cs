using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using Rockyfi;
using WinFormBridge;

namespace Test
{
    public class Example01_ElementController : ElementController
    {
        public Example01_ElementController(string tagName) : base(tagName)
        {
        }

        public override void Draw()
        {
            var rect = Rect;

            //Graphics.SetColor(fgColor);
            //Graphics.Print(TagName + Text, rect.X, rect.Y);
            //Graphics.Rectangle(DrawMode.Line, rect);
            //Graphics.SetColor(Color.White);
        }

        Color fgColor = Color.White;

        public override void UpdateInputHoverVisible()
        {
            //if (Mouse.IsReleased(Mouse.LeftButton))
            //{
            //    Console.WriteLine("click me !" + this);
            //}
        }

        public override void UpdateInputHover()
        {
            fgColor = Color.Red;
        }

        public override void UpdateInputNotHover()
        {
            fgColor = Color.White;
        }

        public override void UpdateInputHoverEnter()
        {
        }

        public override void UpdateInputHoverLeave()
        {
        }

        public override void UpdateInputAutoNavigation()
        {
            fgColor = Color.Green;
        }
    }


    public class Example01_LayoutController : LayoutController
    {
        public override ElementController CreateElement(string tagName, Dictionary<string, object> attr)
        {
            return new Example01_ElementController(tagName);
        }

        public override Dictionary<string, object> DefineInitialData()
        {
            return new Dictionary<string, object>
            {
                {"w", 0 },
                {"h", 0 },
                {"pd", 0.01f },
                {"listData", new int[]{ } },
            };
        }

        public override string DefineLayoutDocument()
        {
            return @"
<root

    el-bind:width='w * (1 - pd*2)' el-bind:height='h * (1 - pd*2)'
    el-bind:margin-left='w * pd'  el-bind:margin-right='w * pd'
    el-bind:margin-top='h * pd'  el-bind:margin-bottom='h * pd'
    flex-wrap='wrap' flex-direction='row'
    overflow='scroll'

>
    <div width='250px' height='150px' position='absolute' > absolute </div>
    <div autoNavigation='true' el-for='itemId in listData' width='150px' height='100px' el-bind:id='itemId'  margin='100' > {{itemId}}
        <div width='30%' height='30%'  />
    </div>
</root>
";
        }

        public override void SetDataToUpdate()
        {
            //SetData("w", Graphics.GetWidth());
            //SetData("h", Graphics.GetHeight());
            SetData("w", 800);
            SetData("h", 600);

            SetData("listData", new List<string>
            {
                "child-0", "child-1", "child-2", "child-3", "child-4",
            });

            base.Update();
        }
    }

    public class RockyTest01
    {
        Example01_LayoutController rtlc = new Example01_LayoutController();

        //public override void Update(float dt)
        //{
        //    base.Update(dt);
        //    rtlc.Update();
        //}

        //public override void Draw()
        //{
        //    base.Draw();
        //    rtlc.Draw();
        //}

        /// <summary>
        /// 应用程序的主入口点。
        /// </summary>
        [STAThread]
        static void Main()
        {
            Application.EnableVisualStyles();
            Application.SetCompatibleTextRenderingDefault(false);
            Application.Run(new RockyfiWinForm(null));
        }
    }
}
