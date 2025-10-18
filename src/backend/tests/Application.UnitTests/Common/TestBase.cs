using AutoMapper;
using backend.Application.Common.Interfaces;
using backend.Application.Identity.UserProfiles.Queries;
using backend.Infrastructure.Data;
using MediatR;
using Moq;

namespace backend.Application.UnitTests.Common;

public abstract class TestBase : IDisposable
{
    protected readonly ApplicationDbContext _context;
    protected readonly IMapper _mapper;
    protected readonly Mock<IUser> _mockUser;
    protected readonly Mock<IAuthorizationService> _mockAuthorizationService;
    protected readonly Mock<IMediator> _mockMediator;
    protected readonly Mock<IFamilyTreeService> _mockFamilyTreeService;

    protected TestBase()
    {
        _context = TestDbContextFactory.Create();

        var configurationProvider = new MapperConfiguration(cfg =>
        {
            cfg.AddMaps(typeof(MappingProfile).Assembly);
        });
        _mapper = configurationProvider.CreateMapper();

        _mockUser = new Mock<IUser>();
        _mockAuthorizationService = new Mock<IAuthorizationService>();
        _mockMediator = new Mock<IMediator>();
        _mockFamilyTreeService = new Mock<IFamilyTreeService>();
    }

    public void Dispose()
    {
        TestDbContextFactory.Destroy(_context);
    }
}
