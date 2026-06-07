using ServiceAbstraction.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ServiceAbstraction.Interfaces
{
    public interface IChatBotService
    {
       
        Task<ChatMessageResponseDto> SendMessageAsync(string userId, ChatMessageRequestDto dto);
    }
}
