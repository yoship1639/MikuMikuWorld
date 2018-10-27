using Microsoft.CodeAnalysis.CSharp.Scripting;
using Microsoft.CodeAnalysis.Scripting;
using MikuMikuWorld.Scripts;
using OpenTK;
using OpenTK.Graphics;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;

namespace MikuMikuWorld
{
    public partial class ScriptForm : Form
    {
        public ScriptForm()
        {
            InitializeComponent();
        }

        private void button_run_Click(object sender, EventArgs e)
        {
            button_run.Enabled = false;
            textBox_out.Clear();
            textBox_out.AppendText("Compile Script...");
            textBox_out.AppendText(Environment.NewLine);

            ScriptOptions options = ScriptOptions.Default
                .WithImports("OpenTK", "OpenTK.Graphics")
                .WithReferences(Assembly.GetAssembly(typeof(Vector2)))
                .WithReferences(Assembly.GetAssembly(typeof(Vector3)))
                .WithReferences(Assembly.GetAssembly(typeof(Vector4)))
                .WithReferences(Assembly.GetAssembly(typeof(Matrix4)))
                .WithReferences(Assembly.GetAssembly(typeof(Color4)));

            var script = CSharpScript.Create(textBox_script.Text, options, typeof(Scripting));

            try
            {
                var task = script.RunAsync(new Scripting());
                task.Wait();
                textBox_out.AppendText("OK");
            }
            catch (CompilationErrorException ex)
            {
                textBox_out.AppendText(ex.ToString());
                textBox_out.AppendText(Environment.NewLine);
            }

            button_run.Enabled = true;
        }
    }
}
