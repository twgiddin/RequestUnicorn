using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;


using Amazon.Lambda.Core;

// Assembly attribute to enable the Lambda function's JSON input to be converted into a .NET class.
[assembly: LambdaSerializer(typeof(Amazon.Lambda.Serialization.Json.JsonSerializer))]

namespace RequestUnicorn
{
    public class Function
    {

        public  List<Unicorn> fleet = new List<Unicorn>();

         /// <summary>
        /// A simple function that takes a string and does a ToUpper
        /// </summary>
        /// <param name="input"></param>
        /// <param name="context"></param>
        /// <returns></returns>
        /// 

            //Make a list of random unicorns
        private void LoadUnicorns()
        {
            Unicorn bucephalus = new Unicorn() { Name = "Bucephalus", Color = "Golden", Gender = "Male" };
            Unicorn shadowfax = new Unicorn() { Name = "Shadowfax", Color = "White", Gender = "Male" };
            Unicorn rocinante = new Unicorn() { Name = "Rocinante", Color = "Yellow", Gender = "Female" };
            Unicorn homer = new Unicorn() { Name = "Homer", Color = "Snow White", Gender = "Male" };
            fleet.Add(bucephalus);
            fleet.Add(homer);
            fleet.Add(shadowfax);
            fleet.Add(rocinante);

        }

        //Key thing, you get in an APIGatewayProxyRequest object since you are using API Gateway to fire the request and it expects
        //an APIGatewayProxyResponse object back
        //This code mimics the original node.js code, just ported to be in .NET for an example
        public Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse FunctionHandler(Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest input, ILambdaContext context)
        {
            //Load the static list of unicorns
            LoadUnicorns();
            //pre-build the response
            Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse response = new Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse();
            //The node.js code has this set for CORS so we add it to the response
            response.Headers = new Dictionary<string, string>();
            response.Headers["Access-Control-Allow-Origin"] =  "*";

            //if the user does not have an authorization, throw a 500 error, just like the original code
            if (input?.RequestContext?.Authorizer == null)
            {
                response.StatusCode = 500;
                var em = new ErrorReturn();
                em.Reference = context?.AwsRequestId;
                em.Error = "Authorization not configured";
                response.Body = Newtonsoft.Json.JsonConvert.SerializeObject(em);

                return response;
            }
            //Make a random long key for lookups
            string rideId = Guid.NewGuid().ToString().Replace("-", "");

            //When you write to the console in Lambda the information will show up in CloudWatch Logs
            Console.WriteLine($"Received request for ride {rideId}");

            // Because we're using a Cognito User Pools authorizer, all of the claims
            // included in the authentication token are provided in the request context.
            // This includes the username as well as other attributes.
            string username = input?.RequestContext?.Authorizer?.Claims ["cognito:username"];

            // The body field of the event in a proxy integration is a raw string.
            // In order to extract meaningful values, we need to first parse this string
            // into an object. A more robust implementation might inspect the Content-Type
            // header first and use a different parsing strategy based on that value.

            
           try
            {
                //Use Newtonsoft to take the body of the input and make it a location object
                var requestedLocation = Newtonsoft.Json.JsonConvert.DeserializeObject<PickupLocationBody>(input.Body);
                
                //send in the pickuplocation attribute to the findUnicorn method to get back your unicorn
                var unicorn = findUnicorn(requestedLocation?.PickupLocation);
                //now record the ride in DynamoDB
               var ride= recordRide(rideId, username, unicorn);
                //set the status code
                response.StatusCode = 201;
                //serialize the response (ride) as the body
                response.Body = Newtonsoft.Json.JsonConvert.SerializeObject(ride);

            }
            catch (Exception ex)
            {
                //if you get an exception, just write it to console and cloudwatch logs will show it in the log
                Console.WriteLine(ex.Message);
                response.StatusCode = 500;
                var em = new ErrorReturn();
                em.Reference = context?.AwsRequestId;
                em.Error = ex.Message;
                response.Body = Newtonsoft.Json.JsonConvert.SerializeObject(em);
                
            }

            return response;
        }


        private Unicorn findUnicorn(PickupLocation pickupLocation)
        {

            //Same as the node.js code, just grab a random one from the fleet array
            Console.WriteLine($"Finding unicorn for {pickupLocation.Latitude}, {pickupLocation.Longitude}");
            return fleet[(new Random().Next(0, fleet.Count))];
            
        }

        private RideResponse recordRide(string rideId, string username, Unicorn unicorn)
        {
            //The client represents a connection to dynamo db's management plane
            using (var  client = new Amazon.DynamoDBv2.AmazonDynamoDBClient()){
                //create a data context
                var context = new Amazon.DynamoDBv2.DataModel.DynamoDBContext(client);
                //populate the ride object
                Ride r = new Ride();
                r.Unicorn = unicorn;
                r.RideId = rideId;
                r.User = username;
                r.RequestTime = DateTime.UtcNow;
                r.UnicornName = unicorn.Name;
                //Now persist it to DynamoDB, it is async but here we are waiting so that we know it has completed or failed
                context.SaveAsync(r).GetAwaiter().GetResult();
                //now build the response object
                var rr = new RideResponse();
                rr.ETA = $"30 seconds";
                rr.Unicorn = unicorn;
                rr.RideId = rideId;
                rr.Rider = username;
                rr.UnicornName = unicorn?.Name;
               
                return rr;

            }
        }
    }

       
    }

