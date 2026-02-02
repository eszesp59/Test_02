using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using SwaggerWcf.Attributes;
using System.ComponentModel;

namespace RESTWcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        [WebGet(UriTemplate = "GetData/{value}", ResponseFormat = WebMessageFormat.Json)]
        [SwaggerWcfPath("Service1", "GetData")]
        [Description("Gets a string message based on the input integer.")]
        string GetData(int value);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "GetDataUsingDataContract", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [SwaggerWcfPath("Service1", "GetDataUsingDataContract")]
        [Description("Processes a composite type data contract.")]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        // TODO: Add your service operations here
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    [Description("A composite type for data transfer.")]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        [Description("A boolean value.")]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        [Description("A string value.")]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}
