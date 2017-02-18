using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using ChronobankApi.Models;
using Core.Contracts;
using Core.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ChronobankApi.Controllers
{
    [Route("api/[controller]")]
    public class ClientController : Controller
    {
        private readonly IUserContractQueueService _userContractQueueService;

        public ClientController(IUserContractQueueService userContractQueueService)
        {
            _userContractQueueService = userContractQueueService;
        }

        [HttpGet("register")]
        [Produces(typeof(RegisterResponse))]
        public async Task<IActionResult> Register()
        {
            var contract = await _userContractQueueService.GetContract();

            var response = new RegisterResponse
            {
                Contract = contract
            };

            return Ok(response);
        }
    }
}
