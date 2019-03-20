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
        public Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse FunctionHandler(Amazon.Lambda.APIGatewayEvents.APIGatewayProxyRequest input, ILambdaContext context)
        {
            LoadUnicorns();
            Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse response = new Amazon.Lambda.APIGatewayEvents.APIGatewayProxyResponse();
            response.Headers = new Dictionary<string, string>();
            response.Headers["Access-Control-Allow-Origin"] =  "*";

            if (input?.RequestContext?.Authorizer == null)
            {
                response.StatusCode = 500;
                var em = new ErrorReturn();
                em.Reference = context?.AwsRequestId;
                em.Error = "Authorization not configured";
                response.Body = Newtonsoft.Json.JsonConvert.SerializeObject(em);

                return response;
            }

            string rideId = Guid.NewGuid().ToString().Replace("-", "");

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
                var requestedLocation = Newtonsoft.Json.JsonConvert.DeserializeObject<PickupLocationBody>(input.Body);
                

                var unicorn = findUnicorn(requestedLocation?.PickupLocation);
               var ride= recordRide(rideId, username, unicorn);
                response.StatusCode = 201;

                response.Body = Newtonsoft.Json.JsonConvert.SerializeObject(ride);

            }
            catch (Exception ex)
            {
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


            Console.WriteLine($"Finding unicorn for {pickupLocation.Latitude}, {pickupLocation.Longitude}");
            return fleet[(new Random().Next(0, fleet.Count))];
            
        }

        private RideResponse recordRide(string rideId, string username, Unicorn unicorn)
        {
            using (var  client = new Amazon.DynamoDBv2.AmazonDynamoDBClient()){

                var context = new Amazon.DynamoDBv2.DataModel.DynamoDBContext(client);
                Ride r = new Ride();
                r.Unicorn = unicorn;
                r.RideId = rideId;
                r.User = username;
                r.RequestTime = DateTime.UtcNow;
                r.UnicornName = unicorn.Name;
                context.SaveAsync(r).GetAwaiter().GetResult();
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

