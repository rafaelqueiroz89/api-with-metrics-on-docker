﻿using Checkout.PaymentGateway.Domain;
using Checkout.PaymentGateway.Domain.PaymentAggregate;

using MediatR;

namespace Checkout.PaymentGateway.CQRS.Commands
{
    /// <summary>
    /// Request payment command
    /// </summary>
    /// <seealso cref="MediatR.IRequest{Checkout.PaymentGateway.Domain.PaymentAggregate.Payment}" />
    public class RequestPaymentCommand : IRequest<BankResponsePayment>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RequestPaymentCommand"/> class.
        /// </summary>
        /// <param name="requestPaymentAggregate">The request payment aggregate.</param>
        public RequestPaymentCommand(RequestPayment requestPayment)
        {
            this.RequestPayment = requestPayment;
        }

        /// <summary>
        /// Gets the request payment aggregate.
        /// </summary>
        /// <value>
        /// The request payment aggregate.
        /// </value>
        public RequestPayment RequestPayment { get; private set; }
    }
}