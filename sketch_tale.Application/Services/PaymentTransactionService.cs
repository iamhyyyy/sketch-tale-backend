using AutoMapper;
using Microsoft.AspNetCore.Http.HttpResults;
using Microsoft.AspNetCore.Mvc;
using sketch_tale.Application.DTOs;
using sketch_tale.Application.Interfaces;
using sketch_tale.Application.Interfaces.Repositories;
using sketch_tale.Application.Interfaces.Services;
using sketch_tale.Domain.Entities;
using sketch_tale.Domain.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace sketch_tale.Application.Services
{
    public class PaymentTransactionService : IPaymentTransactionService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;
        private readonly IPayOsService _payOsService;
        private readonly ISubscriptionService _subscriptionService;

        public PaymentTransactionService (IUnitOfWork unitOfWork,
            IMapper mapper,
            IPayOsService payOsService,
            ISubscriptionService subscriptionService)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
            _payOsService = payOsService;
            _subscriptionService = subscriptionService;
        }

        public async Task<bool> CancelPayment(long orderCode, long code)
        {
            var transaction = await _unitOfWork.PaymentTransactionRepository.GetByOrderCodeAsync(orderCode.ToString());

            if (transaction == null) throw new Exception("Không tìm thấy giao dịch! "); ;

            if (transaction.Status == PaymentStatus.Pending && code == 00)
            {
                    transaction.Status = PaymentStatus.Failed;
                    transaction.ResponseCode = code.ToString();
                    transaction.PayDate = DateTime.UtcNow.AddHours(7);
                    _unitOfWork.PaymentTransactionRepository.Update(transaction);

                    await _subscriptionService.Delete(transaction.SubscriptionId);
                    await _unitOfWork.CompleteAsync();
                    return true;
            }
            else
            {
                throw new Exception("Giao dịch đã được xử lý hoặc đã bị hủy.");
            }
        }

        public async Task<List<PaymentTransactionDto>> GetAllWithDetailsAsync()
        {
            var paymentList = await _unitOfWork.PaymentTransactionRepository.GetAllWithDetailsAsync();
            return _mapper.Map<List<PaymentTransactionDto>>(paymentList);
        }

        public async Task<PaymentTransactionDto> GetByIdWithDetailsAsync(Guid id)
        {
            var payment = await _unitOfWork.PaymentTransactionRepository.GetByIdWithDetailsAsync(id);
            return _mapper.Map<PaymentTransactionDto>(payment);
        }

        public async Task<PaymentTransactionDto> GetByOrderCodeAsync(string orderCode)
        {
            var payment = await _unitOfWork.PaymentTransactionRepository.GetByOrderCodeAsync(orderCode);
            return _mapper.Map<PaymentTransactionDto>(payment);
        }
    }
}
