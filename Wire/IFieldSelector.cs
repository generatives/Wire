using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using Wire.Extensions;

namespace Wire
{
    public interface IFieldSelector
    {
        FieldInfo[] SelectFields(Type type);
    }

    public class DefaultFieldSelector : IFieldSelector
    {
        public FieldInfo[] SelectFields(Type type)
        {
            return type.GetFieldInfosForType();
        }
    }
}
