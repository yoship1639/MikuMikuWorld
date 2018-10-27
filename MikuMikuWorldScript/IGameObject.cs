using OpenTK;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace MikuMikuWorldScript
{
    public interface IGameObject : IParam, IMethod
    {
        string Name { get; }
        string Hash { get; }
        //int Layer { get; set; }
        List<string> Tags { get; }
        bool Enabled { get; set; }

        IGameComponent Transform { get; }
        IGameComponent[] FindGameComponents(Predicate<IGameComponent> match);

        T SendRequest<T>(string request, params object[] args);
        void SendMessage(string message, params object[] args);
    }
}
