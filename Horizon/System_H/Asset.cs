using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace System_H
{
    class Asset
    {
        protected Position _assetPosition{get; private set};
        protected bool _isTaskable{get; private set};

        public Asset() {
            _isTaskable = false;
        }
        public Asset(Position pos)
        {
            _assetPosition = pos;
            _isTaskable = false;
        }

        public Asset(XMLNode XML pos)
        {
            _assetPosition =new Position(assetXMLNode.getChildNode("POSITION");
            _isTaskable = false;
        }

    }
}
