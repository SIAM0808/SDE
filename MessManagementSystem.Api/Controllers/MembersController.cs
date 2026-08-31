using MessManagementSystem.Api.Models;
using Microsoft.AspNetCore.Mvc;

namespace MessManagementSystem.Api.Controllers;

using MessManagementSystem.Api.Models;
using MessManagementSystem.Api.Data;
using MessManagementSystem.Api.DTOs;
using Microsoft.AspNetCore.Authorization;
using System.Security.Claims;
using Microsoft.EntityFrameworkCore;

[ApiController]
[Route("api/[controller]")]


public class MembersController : ControllerBase
{
    // In-memory storage for members

    private readonly MessDbContext _context;

    public MembersController(MessDbContext context)
    {
        _context = context;
    }
    // GET: api/members
    [HttpGet]
    public IActionResult GetMembers()
    {
        var members = _context.Members.ToList();
        return Ok(members);
    }


    [HttpGet("{id}")]
    public IActionResult GetMember(int id)
    {
        var member = _context.Members.FirstOrDefault(m => m.Id == id);

        if (member == null)
        {
            return NotFound("Member not found");
        }

        return Ok(member);
    }

    // POST: api/members
    [HttpPost]
    public IActionResult CreateMember(CreateMemberDto dto)
    {
        var member = new Member
        {
            Name = dto.Name,
            Phone = dto.Phone,
            Email = dto.Email,
            JoinDate = DateTime.Now,
            IsActive = true
        };
        _context.Members.Add(member);
        _context.SaveChanges();
        return Ok(member);
    }

    // PUT: api/members/{id}
    [HttpPut("{id}")]
    public IActionResult UpdateMember(int id, UpdateMemberDto dto)
    {
        var member = _context.Members.FirstOrDefault(m => m.Id == id);

        if (member == null)
        {
            return NotFound();
        }

        member.Name = dto.Name;
        member.Phone = dto.Phone;
        member.Email = dto.Email;
        _context.SaveChanges();
        return Ok(member);
    }


    // DELETE: api/members/{id}
    [HttpDelete("{id}")]
    public IActionResult DeleteMember(int id)
    {
        var member = _context.Members.FirstOrDefault(m => m.Id == id);

        if (member == null)
        {
            return NotFound();
        }

        member.IsActive = false;
        _context.SaveChanges();

        return Ok(member);
    }

    [Authorize]
    [HttpGet("me")]
    public async Task<IActionResult> GetCurrentMember()
    {
        var memberIdValue = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (memberIdValue == null)
        {
            return Unauthorized();
        }

        var memberId = int.Parse(memberIdValue);

        var member = await _context.Members
            .Include(m => m.Mess)
            .FirstOrDefaultAsync(m => m.Id == memberId);

        if (member == null)
        {
            return NotFound(new
            {
                message = "Member not found."
            });
        }

        return Ok(new
        {
            memberId = member.Id,
            name = member.Name,
            email = member.Email,
            messId = member.MessId,
            messName = member.Mess?.Name
        });
    }
}