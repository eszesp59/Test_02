using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.Serialization;
using System.ServiceModel;
using System.ServiceModel.Web;
using System.Text;
using System.ComponentModel;
using SwaggerWcf.Attributes;

namespace RESTWcfService
{
    // NOTE: You can use the "Rename" command on the "Refactor" menu to change the interface name "IService1" in both code and config file together.
    [ServiceContract]
    public interface IService1
    {

        [OperationContract]
        [WebGet(UriTemplate = "GetData/{value}", ResponseFormat = WebMessageFormat.Json)]
        [SwaggerWcfPath("Get Data", "Retrieves a formatted string based on the input integer.")]
        string GetData(int value);

        [OperationContract]
        [WebInvoke(Method = "POST", UriTemplate = "GetDataUsingDataContract", RequestFormat = WebMessageFormat.Json, ResponseFormat = WebMessageFormat.Json)]
        [SwaggerWcfPath("Get Data Using Data Contract", "Processes a composite type and returns the modified object.")]
        CompositeType GetDataUsingDataContract(CompositeType composite);

        // TODO: Add your service operations here
    }


    // Use a data contract as illustrated in the sample below to add composite types to service operations.
    [DataContract]
    public class CompositeType
    {
        bool boolValue = true;
        string stringValue = "Hello ";

        [DataMember]
        [Description("Indicates whether the string value should be suffixed.")]
        public bool BoolValue
        {
            get { return boolValue; }
            set { boolValue = value; }
        }

        [DataMember]
        [Description("The string value to be processed.")]
        public string StringValue
        {
            get { return stringValue; }
            set { stringValue = value; }
        }
    }
}
