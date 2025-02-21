using CourtBooking.Application.DTOs;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace CourtBooking.Application.CourtManagement.Command.UpdateCourt;

public record UpdateCourtCommand(CourtUpdateDTO Court) : ICommand<UpdateCourtResult>;
public record UpdateCourtResult(bool IsSuccess);
