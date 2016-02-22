using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Universe;
namespace HSFSystem
{
    public class Asset
    {
        public DynamicState _assetPosition{ get; private set; } //was protected, why?
        //TODO:make isTaskable mean something
        public bool _isTaskable{ get; private set; }//was protected, why?

        public Asset() {
            _isTaskable = false;
        }
        public Asset(DynamicState pos)
        {
            _assetPosition = pos;
            _isTaskable = false;
        }

        public Asset(XMLNode assetXMLNode)
        {
            _assetPosition =new DynamicState(assetXMLNode.getChildNode("POSITION"));
            _isTaskable = false;
        }

    }
}
