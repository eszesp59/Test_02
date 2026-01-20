using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.ComponentModel;

namespace RESTWcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        [WebGet(UriTemplate = "GetData/{value}", ResponseFormat = WebMessageFormat.Json)]
        [Description("Gets a string value based on the input integer.")]
        string GetData(int value);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "GetDataUsingDataContract", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [Description("Processes a composite type, appending 'Suffix' if BoolValue is true.")]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        // TODO: Add your service operations here
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    [Description("A composite type containing a boolean and a string.")]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        [Description("A boolean value that controls processing logic.")]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        [Description("A string value that may be modified.")]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}
