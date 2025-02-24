using HotChocolate;
using System.Threading.Tasks;
using CabinetBooking; // Пространство имён, сгенерированное из .proto файла
using Grpc.Net.Client;

public class Mutation
{
    private readonly CabinetBookingService.CabinetBookingServiceClient _grpcClient;

    public Mutation(CabinetBookingService.CabinetBookingServiceClient grpcClient)
    {
        _grpcClient = grpcClient;
    }

    public async Task<string> AddCabinet(
        string number,
        bool isTechnical,
        bool isProjector,
        CabinetType cabinetType)
    {
        // Создаём запрос для gRPC сервиса
        var request = new AddCabinetRequest
        {
            Cabinet = new Cabinet
            {
                Number = number,
                IsTechnical = isTechnical,
                IsProjector = isProjector,
                CabinetType = cabinetType
            }
        };

        // Вызываем gRPC сервис
        var response = await _grpcClient.AddCabinetAsync(request);

        // Возвращаем результат
        return response.Number;
    }
}