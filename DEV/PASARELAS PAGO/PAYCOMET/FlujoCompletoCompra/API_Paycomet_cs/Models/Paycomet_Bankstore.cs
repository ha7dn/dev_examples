using System;
using System.Collections.Generic;
using System.Web;
using System.Text.RegularExpressions;
using System.Net;
using API_Paycomet_cs.api.paycomet.com;
using API_Paycomet_cs.api.paycomet.com_2;
using System.Security.Cryptography;
using System.Globalization;

namespace API_Paycomet_cs.Models
{
    public class Paycomet_Bankstore
    {
        private Regex regEx = new Regex(@"\s+");
        private string merchantCode;
        private string terminal;
        private string password;
        private string endpoint;
        private string endpointUrl;
        private string jetId;
        public OperationData OperationData = new OperationData();

        public Paycomet_Bankstore(string merchantCode, string terminal, string password, string endpoint, string endpointUrl, string jetId = null)
        {
            ServicePointManager.SecurityProtocol = SecurityProtocolType.Tls12;
            this.merchantCode = merchantCode;
            this.terminal = terminal;
            this.password = password;
            this.jetId = jetId;
            this.endpoint = endpoint;
            this.endpointUrl = endpointUrl;
        }

        private string operationAuthCode(string transReference, string currency, string operation)
        {
            string[] terminals = { terminal };
            string[] operations = { operation };
            DateTime dt = DateTime.Now;
            string formatstart = "yyyyMMdd000000";
            string formatend = "yyyyMMdd235959";
            string datestart;
            string dateend;
            string result;

            PAYTPV_OperationsGatewayService opWsProxy = new PAYTPV_OperationsGatewayService();

            datestart = dt.ToString(formatstart, DateTimeFormatInfo.InvariantInfo);
            dateend = dt.ToString(formatend, DateTimeFormatInfo.InvariantInfo);

            string opSignature = Cryptography.SHA512HashStringForUTF8String(
                   merchantCode + terminal + password + operation + datestart + dateend
            );

            Operation[] op = opWsProxy.search_operations(
                merchantCode,
                "0",
                "ASC",
                "1",
                terminals,
                operations,
                "0",
                "9999999",
                "1",
                datestart,
                dateend,
                currency,
                opSignature,
                transReference,
                "1",
                "1.10"
            );

            result = op[0].PAYTPV_OPERATION_AUTHCODE;
            return result;
        }

        /// <summary>
        /// Add a card to PayCOMET.  IMPORTANT !!! This direct input must be activated by PayCOMET.
        /// In default input method card for PCI-DSS compliance should be AddUserUrl or AddUserToken (method used by BankStore JET)
        /// </summary>
        /// <param name="pan">card number without spaces or dashes</param>
        /// <param name="expDate">expDate expiry date of the card, expressed as "MMYY" (two-digit month and year in two digits)</param>
        /// <param name="cvv">CVC2 Card code</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse AddUser(string pan, string expDate, string cvv, string ipClient)
        {
            BankstoreServResponse result = new BankstoreServResponse();
            pan = regEx.Replace(pan, string.Empty);
            expDate = regEx.Replace(expDate, string.Empty);
            cvv = regEx.Replace(cvv, string.Empty);

            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + pan + cvv + terminal + password);
            try
            {

                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();


                string tokenUser, dsErrorId = string.Empty;
                string idUser = wsProxy.add_user(merchantCode, terminal, pan, expDate, cvv, signature, ipClient, "Test name", out tokenUser, out dsErrorId);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_IDUSER", idUser);
                    result.Data.Add("DS_TOKEN_USER", tokenUser);
                    result.Result = "OK";
                }
                return result;
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception e)
            {
                result.DsErrorId = e.Message;
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Returns the user information stored in a call PayCOMET by soap
        /// </summary>
        /// <param name="idPayUser">User ID in PayCOMET</param>
        /// <param name="tokenPayUser">tokenPayUser user Token in PayCOMET</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse InfoUser(string idPayUser, string tokenPayUser, string ipClient)
        {

            BankstoreServResponse result = new BankstoreServResponse();
            idPayUser = regEx.Replace(idPayUser, string.Empty);
            tokenPayUser = regEx.Replace(tokenPayUser, string.Empty);

            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + idPayUser + tokenPayUser + terminal + password);

            try
            {
                string dscardBrand, dsCardType, card1CountryISO3, cardExpiryDate, dsErrorId, dsCardHash, dsCardCategory, dsSepaCard = string.Empty;
                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();

                string merchantPAN = wsProxy.info_user(merchantCode, terminal, idPayUser, tokenPayUser, signature, ipClient, out dsErrorId,
                    out dscardBrand, out dsCardType, out card1CountryISO3, out cardExpiryDate, out dsCardHash, out dsCardCategory, out dsSepaCard);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_MERCHANT_PAN", merchantPAN);
                    result.Data.Add("DS_CARD_BRAND", dscardBrand);
                    result.Data.Add("DS_CARD_TYPE", dsCardType);
                    result.Data.Add("DS_CARD_I_COUNTRY_ISO3", card1CountryISO3);
                    result.Data.Add("DS_EXPIRYDATE", cardExpiryDate);
                    result.Result = "OK";
                }

            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Removes a user through call soap PayCOMET
        /// </summary>
        /// <param name="idPayUser">user ID PayCOMET</param>
        /// <param name="tokenPayUser">User Token PayCOMET</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <returns>Object A transaction response</returns>
        public BankstoreServResponse RemoveUser(string idPayUser, string tokenPayUser, string ipClient)
        {
            BankstoreServResponse result = new BankstoreServResponse();
            idPayUser = regEx.Replace(idPayUser, string.Empty);
            tokenPayUser = regEx.Replace(tokenPayUser, string.Empty);
            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + idPayUser + tokenPayUser + terminal + password);

            try
            {
                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();
                string dsErrorId = string.Empty;
                var dsResponse = wsProxy.remove_user(merchantCode, terminal, idPayUser, tokenPayUser, signature, ipClient, out dsErrorId);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data["DS_RESPONSE"] = dsResponse;
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Execute a web service payment
        /// </summary>
        /// <param name="idPayUser">User ID in PayCOMET</param>
        /// <param name="tokenPayUser">user Token in PayCOMET</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="transReference">unique identifier payment</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="productDescription">Product Description Product Description</param>
        /// <param name="owner">owner Cardholder</param>
        /// <param name="scoring">Risk scoring value of the transaction</param>
        /// <param name="merchant_data">Trade Data</param>
        /// <param name="merchant_description">Trade Descriptor</param>
        /// <param name="sca_exception">Optional TYPE OF EXCEPTION TO INSURANCE PAYMENT.</param>
        /// <param name="trx_type">(conditional) Required only if an MIT exception has been chosen in the field MERCHANT_SCA_EXCEPTION</param>
        /// <param name="scrow_targets">(optional) Identification of revenue recipients in ESCROW operations</param>
        /// <param name="user_interaction">(optional) Indicator of whether interaction with the user by the retailer is possible</param>
        /// <returns>transaction response</returns>     
        public BankstoreServResponse ExecutePurchase(string idPayUser, string tokenPayUser, string ipClient, string amount, string transReference, string currency, 
                                                    string productDescription = null, string owner = null, string scoring = null, string merchant_data = null, string merchant_description = null, 
                                                    string sca_exception = null, string trx_type = null, string scrow_targets = null, string user_interaction = null)
        {
            BankstoreServResponse result = new BankstoreServResponse();
            idPayUser = regEx.Replace(idPayUser, string.Empty);
            tokenPayUser = regEx.Replace(tokenPayUser, string.Empty);
            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + idPayUser + tokenPayUser + terminal + amount + transReference + password);

            try
            {
                string dsErrorId, dsMerchantCardCountry, dsResponse, dsUrl = string.Empty;

                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();

                string authcode = wsProxy.execute_purchase(merchantCode,
                                                            terminal,
                                                            idPayUser,
                                                            tokenPayUser,
                                                            ref amount,
                                                            ref transReference,
                                                            ref currency,
                                                            signature,
                                                            ipClient,
                                                            productDescription,
                                                            owner,
                                                            scoring,
                                                            merchant_data,
                                                            merchant_description,
                                                            sca_exception,
                                                            trx_type,
                                                            scrow_targets,
                                                            user_interaction,
                                                            out dsMerchantCardCountry,
                                                            out dsResponse,
                                                            out dsErrorId,
                                                            out dsUrl);


                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_MERCHANT_AMOUNT", amount);
                    result.Data.Add("DS_MERCHANT_ORDER", transReference);
                    result.Data.Add("DS_MERCHANT_CURRENCY", currency);
                    result.Data.Add("DS_MERCHANT_AUTHCODE", this.operationAuthCode(transReference, currency, "1"));
                    result.Data.Add("DS_MERCHANT_CARDCOUNTRY", dsMerchantCardCountry);
                    result.Data.Add("DS_RESPONSE", dsResponse);
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Execute a web service payment with DCC operational
        /// </summary>
        /// <param name="idPayUser">idPayUser User ID in PayCOMET</param>
        /// <param name="tokenPayUser">tokenPayUser user Token in PayCOMET</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="productDescription">Product Description Product Description</param>
        /// <param name="owner">owner Cardholder</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse ExecutePurchaseDcc(string idPayUser, string tokenPayUser, string ipClient, string amount, string transReference, string productDescription = null, string owner = null)
        {
            BankstoreServResponse result = new BankstoreServResponse();
            idPayUser = regEx.Replace(idPayUser, string.Empty);
            tokenPayUser = regEx.Replace(tokenPayUser, string.Empty);
            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + idPayUser + tokenPayUser + terminal + amount + transReference + password);

            try
            {
                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();
                string dsErrorId, dsMerchantDccSession, dsMerchantDccCurrency, dsMerchantDccCurrencyIso3, dsMerchantDccCurrencyName, dsMerchantDescription = string.Empty;
                string dsMerchantDccExchange, dsMerchantDccMarkup, dsMerchantDccCardCountry, dsResponse = string.Empty;
                string dsMerchantDccAmount = string.Empty;
                string merchantCurrency = wsProxy.execute_purchase_dcc(
                    merchantCode,
                    terminal,
                    idPayUser,
                    tokenPayUser,
                    ref amount,
                    ref transReference,
                    signature,
                    ipClient,
                    productDescription,
                    owner,
                    dsMerchantDescription,
                    out dsMerchantDccSession,
                    out dsMerchantDccCurrency,
                    out dsMerchantDccCurrencyIso3,
                    out dsMerchantDccCurrencyName,
                    out dsMerchantDccExchange,
                    out dsMerchantDccAmount,
                    out dsMerchantDccMarkup,
                    out dsMerchantDccCardCountry,
                    out dsResponse,
                    out dsErrorId);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_MERCHANT_AMOUNT", amount);
                    result.Data.Add("DS_MERCHANT_ORDER", transReference);
                    result.Data.Add("DS_MERCHANT_CURRENCY", merchantCurrency);
                    result.Data.Add("DS_MERCHANT_DCC_SESSION", dsMerchantDccSession);
                    result.Data.Add("DS_MERCHANT_DCC_CURRENCY", dsMerchantDccCurrency);
                    result.Data.Add("DS_MERCHANT_DCC_CURRENCYISO3", dsMerchantDccCurrencyIso3);
                    result.Data.Add("DS_MERCHANT_DCC_CURRENCYNAME", dsMerchantDccCurrencyName);
                    result.Data.Add("DS_MERCHANT_DCC_EXCHANGE", dsMerchantDccExchange);
                    result.Data.Add("DS_MERCHANT_DCC_AMOUNT", dsMerchantDccAmount);
                    result.Data.Add("DS_MERCHANT_DCC_MARKUP", dsMerchantDccMarkup);
                    result.Data.Add("DS_MERCHANT_DCC_CARDCOUNTRY", dsMerchantDccCardCountry);
                    result.Data.Add("DS_RESPONSE", dsResponse);
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }

            return result;
        }

        /// <summary>
        /// Confirm a payment by web service with DCC operational
        /// </summary>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="dccCurrency">dcccurrency chosen currency transaction. It may be the product of PayCOMET native or selected by the end user. The amount will be sent in execute_purchase_dcc if the same product and become if different.</param>
        /// <param name="dccSession">dccsession sent in the same session execute_purchase_dcc process.</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse ConfirmPurchaseDcc(string transReference, string dccCurrency, string dccSession)
        {
            BankstoreServResponse result = new BankstoreServResponse();
            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + terminal + transReference + dccCurrency + dccSession + password);

            try
            {
                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();

                string dsErrorId, dsMerchantAuthCode, dsMerchantCardCountry = string.Empty;
                string dsMerchantCurrency, dsResponse = string.Empty;
                string dsMerchantAmount = wsProxy.confirm_purchase_dcc(merchantCode, terminal, ref transReference, dccCurrency, dccSession, signature,
                    out dsMerchantCurrency, out dsMerchantAuthCode, out dsMerchantCardCountry, out dsResponse, out dsErrorId);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_MERCHANT_AMOUNT", dsMerchantAmount);
                    result.Data.Add("DS_MERCHANT_ORDER", transReference);
                    result.Data.Add("DS_MERCHANT_CURRENCY", dsMerchantCurrency);
                    result.Data.Add("DS_MERCHANT_AUTHCODE", dsMerchantAuthCode);
                    result.Data.Add("DS_MERCHANT_CARDCOUNTRY", dsMerchantCardCountry);
                    result.Data.Add("DS_RESPONSE", dsResponse);
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Executes a return of a payment web service
        /// </summary>
        /// <param name="idPayUser">User ID in PayCOMET</param>
        /// <param name="tokenPayUser">user Token in PayCOMET</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="authCode">authCode de la operación original a devolver</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse ExecuteRefund(string idPayUser, string tokenPayUser, string ipClient, string transReference, string currency, string authCode, string merchantDescriptor, string amount = null)
        {
            BankstoreServResponse result = new BankstoreServResponse();
            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + idPayUser + tokenPayUser + terminal + authCode + transReference + password);

            try
            {
                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();
                string dsErrorId = string.Empty;

                string dsResponse = wsProxy.execute_refund(merchantCode, terminal, idPayUser, tokenPayUser, ref authCode, ref transReference, ref currency, signature, ipClient, amount, merchantDescriptor, out dsErrorId);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_MERCHANT_ORDER", transReference);
                    result.Data.Add("DS_MERCHANT_CURRENCY", currency);
                    result.Data.Add("DS_MERCHANT_AUTHCODE", authCode);
                    result.Data.Add("DS_RESPONSE", dsResponse);
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Create a subscription in PayCOMET on a card.  IMPORTANTES !!! This direct input must be activated by PayCOMET.
        /// In default input method card for PCI-DSS compliance should be CreateSubscriptionUrl or CreateSubscriptionToken
        /// </summary>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <param name="pan">card number without spaces or dashes</param>
        /// <param name="expDate">expDate expiry date of the card, expressed as "MMYY" (two-digit month and year in two digits)</param>
        /// <param name="cvv">CVC2 Card code</param>
        /// <param name="startDate">startDate date subscription start yyyy-mm-dd</param>
        /// <param name="endDate">endDate Date End subscription yyyy-mm-dd</param>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="periodicity">periodicity Frequency of subscription. In days.</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="sca_exception">Optional TYPE OF EXCEPTION TO INSURANCE PAYMENT.</param>
        /// <param name="trx_type">(conditional) Required only if an MIT exception has been chosen in the field MERCHANT_SCA_EXCEPTION</param>
        /// <param name="scrow_targets">(optional) Identification of revenue recipients in ESCROW operations</param>
        /// <param name="user_interaction">(optional) Indicator of whether interaction with the user by the retailer is possible</param>

        /// <returns>transaction response</returns>
        public BankstoreServResponse CreateSubscription(string ipClient, string pan, string expDate, string cvv, string startDate, string endDate, string transReference, string periodicity, string amount, string currency, string scoring = null, string sca_exception = null, string trx_type = null, string scrow_targets = null, string user_interaction = null)
        {
            BankstoreServResponse result = new BankstoreServResponse();
            pan = regEx.Replace(pan, string.Empty);
            expDate = regEx.Replace(expDate, string.Empty);
            cvv = regEx.Replace(cvv, string.Empty);

            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + pan + cvv + terminal + amount + currency + password);


            try
            {
                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();
                string dsErrorId, dsTokenUser, dsMerchantAuthCode, dsMerchantCardCountry = string.Empty;

                string dsExecute = "1";// Deprecated, default dsExecute = 1 important!!!
                string dsMerchantCardHolderName = string.Empty;
                string dsMerchantData = string.Empty;

                string dsIdUser = wsProxy.create_subscription(merchantCode, terminal, pan, expDate, cvv, startDate, endDate,
                    ref transReference, periodicity, ref amount, ref currency, signature, ipClient, dsExecute, dsMerchantCardHolderName,
                     scoring, dsMerchantData, sca_exception, trx_type, scrow_targets, user_interaction, out dsTokenUser, out dsMerchantAuthCode, out dsMerchantCardCountry, out dsErrorId);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_IDUSER", dsIdUser);
                    result.Data.Add("DS_TOKEN_USER", dsTokenUser);
                    result.Data.Add("DS_SUBSCRIPTION_AMOUNT", amount);
                    result.Data.Add("DS_SUBSCRIPTION_ORDER", transReference);
                    result.Data.Add("DS_SUBSCRIPTION_CURRENCY", currency);
                    result.Data.Add("DS_MERCHANT_AUTHCODE", dsMerchantAuthCode);
                    result.Data.Add("DS_MERCHANT_CARDCOUNTRY", dsMerchantCardCountry);
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Modifies a subscription PayCOMET on a card.
        /// </summary>
        /// <param name="idPayUser">User ID in PayCOMET</param>
        /// <param name="tokenPayUser">user Token in PayCOMET</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <param name="startDate">startDate date subscription start yyyy-mm-dd</param>
        /// <param name="endDate">endDate Date End subscription yyyy-mm-dd</param>
        /// <param name="periodicity">periodicity Frequency of subscription. In days.</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="execute">EXECUTE If the registration process involves the payment of the first installment value DS_EXECUTE should be 1. If you only want to discharge from the subscription without being paid the first installment (will run with the parameters sent) its value must be 0.</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse EditSubscription(string idPayUser, string tokenPayUser, string ipClient, string startDate, string endDate, string periodicity, string amount, string execute)
        {
            BankstoreServResponse result = new BankstoreServResponse();

            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + idPayUser + tokenPayUser + terminal + amount + password);

            try
            {
                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();
                string dsErrorId, dsMerchantCardCountry, dsMerchantAuthCode, dsSubscriptionCurrency = string.Empty;

                string dsSubscriptionOrder = wsProxy.edit_subscription(merchantCode, terminal, ref idPayUser, ref tokenPayUser,
                    startDate, endDate, periodicity, ref amount, signature, execute, ipClient, out dsSubscriptionCurrency,
                    out dsMerchantAuthCode, out dsMerchantCardCountry, out dsErrorId);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();

                    result.Data.Add("DS_IDUSER", idPayUser);
                    result.Data.Add("DS_TOKEN_USER", tokenPayUser);
                    result.Data.Add("DS_SUBSCRIPTION_AMOUNT", amount);
                    result.Data.Add("DS_SUBSCRIPTION_ORDER", dsSubscriptionOrder);
                    result.Data.Add("DS_SUBSCRIPTION_CURRENCY", dsSubscriptionCurrency);
                    result.Data.Add("DS_MERCHANT_AUTHCODE", dsMerchantAuthCode);
                    result.Data.Add("DS_MERCHANT_CARDCOUNTRY", dsMerchantCardCountry);
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Deletes a subscription PayCOMET on a card.
        /// </summary>
        /// <param name="idPayUser">User ID in PayCOMET</param>
        /// <param name="tokenPayUser">user Token in PayCOMET</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse RemoveSubscription(string idPayUser, string tokenPayUser, string ipClient)
        {
            BankstoreServResponse result = new BankstoreServResponse();

            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + idPayUser + tokenPayUser + terminal + password);

            try
            {
                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();
                string dsErrorId = string.Empty;

                string dsResponse = wsProxy.remove_subscription(merchantCode, terminal, idPayUser, tokenPayUser, signature, ipClient, out dsErrorId);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_RESPONSE", dsResponse);
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Create a subscription in PayCOMET on a previously tokenized card.
        /// </summary>
        /// <param name="idPayUser">User ID in PayCOMET</param>
        /// <param name="tokenPayUser">user Token in PayCOMET</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <param name="startDate">startDate date subscription start yyyy-mm-dd</param>
        /// <param name="endDate">endDate Date End subscription yyyy-mm-dd</param>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="periodicity">periodicity Frequency of subscription. In days.</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="sca_exception">Optional TYPE OF EXCEPTION TO INSURANCE PAYMENT.</param>
        /// <param name="trx_type">(conditional) Required only if an MIT exception has been chosen in the field MERCHANT_SCA_EXCEPTION</param>
        /// <param name="scrow_targets">(optional) Identification of revenue recipients in ESCROW operations</param>
        /// <param name="user_interaction">(optional) Indicator of whether interaction with the user by the retailer is possible</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse CreateSubscriptionToken(string idPayUser, string tokenPayUser, string ipClient, string startDate, string endDate, string transReference, string periodicity, string amount, string currency, string scoring = null, string sca_exception = null, string trx_type = null, string scrow_targets = null, string user_interaction = null)
        {
            BankstoreServResponse result = new BankstoreServResponse();

            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + idPayUser + tokenPayUser + terminal + amount
                + currency + password);

            try
            {
                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();

                string dsErrorId, dsMerchantCardCountry = string.Empty;

                string dsMerchantData = string.Empty;

                string authCode = wsProxy.create_subscription_token(merchantCode, terminal, ref idPayUser, ref tokenPayUser, startDate,
                    endDate, ref transReference, periodicity, ref amount, ref currency, signature, ipClient, scoring, dsMerchantData,
                    sca_exception, trx_type, scrow_targets, user_interaction, out dsMerchantCardCountry, out dsErrorId);


                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_IDUSER", idPayUser);
                    result.Data.Add("DS_TOKEN_USER", tokenPayUser);
                    result.Data.Add("DS_SUBSCRIPTION_AMOUNT", amount);
                    result.Data.Add("DS_SUBSCRIPTION_ORDER", transReference);
                    result.Data.Add("DS_SUBSCRIPTION_CURRENCY", currency);
                    result.Data.Add("DS_MERCHANT_AUTHCODE", authCode);
                    result.Data.Add("DS_MERCHANT_CARDCOUNTRY", dsMerchantCardCountry);

                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Create a pre-authorization by web service
        /// </summary>
        /// <param name="idPayUser">User ID in PayCOMET</param>
        /// <param name="tokenPayUser">user Token in PayCOMET</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="productDescription">Product Description Product Description</param>
        /// <param name="owner">owner Cardholder</param>
        /// <param name="sca_exception">Optional TYPE OF EXCEPTION TO INSURANCE PAYMENT.</param>
        /// <param name="trx_type">(conditional) Required only if an MIT exception has been chosen in the field MERCHANT_SCA_EXCEPTION</param>
        /// <param name="scrow_targets">(optional) Identification of revenue recipients in ESCROW operations</param>
        /// <param name="user_interaction">(optional) Indicator of whether interaction with the user by the retailer is possible</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse CreatePreauthorization(string idPayUser, string tokenPayUser, string ipClient, string amount, string transReference, string currency, string productDescription = null, string owner = null, string scoring = null, string sca_exception = null, string trx_type = null, string scrow_targets = null, string user_interaction = null)
        {
            BankstoreServResponse result = new BankstoreServResponse();

            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + idPayUser + tokenPayUser + terminal + amount
                + transReference + password);

            try
            {

                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();
                string dsErrorId, dsMerchantCardCountry, dsResponse = string.Empty;

                string dsMerchantData = string.Empty;
                string dsMerchantDescription = string.Empty;

                string authCode = wsProxy.create_preauthorization(merchantCode, terminal, idPayUser, tokenPayUser, ref amount,
                    ref transReference, ref currency, signature, ipClient, productDescription, owner, scoring, dsMerchantData, dsMerchantDescription,
                    sca_exception, trx_type, scrow_targets, user_interaction, out dsMerchantCardCountry, out dsResponse, out dsErrorId);


                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_MERCHANT_AMOUNT", amount);
                    result.Data.Add("DS_MERCHANT_ORDER", transReference);
                    result.Data.Add("DS_MERCHANT_CURRENCY", currency);
                    result.Data.Add("DS_MERCHANT_AUTHCODE", operationAuthCode(transReference, "EUR", "3"));
                    result.Data.Add("DS_MERCHANT_CARDCOUNTRY", dsMerchantCardCountry);
                    result.Data.Add("DS_RESPONSE", dsResponse);

                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Confirm a pre-authorization previously sent by web service
        /// </summary>
        /// <param name="idPayUser">User ID in PayCOMET</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <param name="tokenPayUser">user Token in PayCOMET</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse PreauthorizationConfirm(string idPayUser, string tokenPayUser, string ipClient, string amount, string transReference)
        {
            BankstoreServResponse result = new BankstoreServResponse();

            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + idPayUser + tokenPayUser + terminal
                + transReference + amount + password);

            try
            {
                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();
                string dsErrorId, dsMerchantAuthCode, dsMerchantCardCountry, dsResponse = string.Empty;
                string dsMerchantDescriptor = string.Empty;

                string dsMerchantCurrency = wsProxy.preauthorization_confirm(merchantCode, terminal, idPayUser, tokenPayUser, ref amount,
                    ref transReference, signature, ipClient, dsMerchantDescriptor, out dsMerchantAuthCode, out dsMerchantCardCountry, out dsResponse,
                    out dsErrorId);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_MERCHANT_AMOUNT", amount);
                    result.Data.Add("DS_MERCHANT_ORDER", transReference);
                    result.Data.Add("DS_MERCHANT_CURRENCY", dsMerchantCurrency);
                    result.Data.Add("DS_MERCHANT_AUTHCODE", dsMerchantAuthCode);
                    result.Data.Add("DS_MERCHANT_CARDCOUNTRY", dsMerchantCardCountry);
                    result.Data.Add("DS_RESPONSE", dsResponse);

                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Cancels a pre-authorization previously sent by web service
        /// </summary>
        /// <param name="idPayUser">User ID in PayCOMET</param>
        /// <param name="tokenPayUser">user Token in PayCOMET</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse PreauthorizationCancel(string idPayUser, string tokenPayUser, string ipClient, string amount, string transReference)
        {
            BankstoreServResponse result = new BankstoreServResponse();

            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + idPayUser + tokenPayUser + terminal + transReference
                + amount + password);

            try
            {
                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();
                string dsErrorId, dsMerchantAuthCode, dsMerchantCardCountry, dsResponse = string.Empty;

                string dsMerchantCurrency = wsProxy.preauthorization_cancel(merchantCode, terminal, idPayUser, tokenPayUser, ref amount,
                    ref transReference, signature, ipClient, out dsMerchantAuthCode, out dsMerchantCardCountry, out dsResponse, out dsErrorId);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_MERCHANT_AMOUNT", amount);
                    result.Data.Add("DS_MERCHANT_ORDER", transReference);
                    result.Data.Add("DS_MERCHANT_CURRENCY", dsMerchantCurrency);
                    result.Data.Add("DS_MERCHANT_AUTHCODE", dsMerchantAuthCode);
                    result.Data.Add("DS_MERCHANT_CARDCOUNTRY", dsMerchantCardCountry);
                    result.Data.Add("DS_RESPONSE", dsResponse);

                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Confirm deferred preauthorization by web service. Once and authorized an operation deferred pre-authorization can be confirmed for the effective recovery within 72 hours; after that date, deferred pre-authorizations lose their validity.
        /// </summary>
        /// <param name="idPayUser">User ID in PayCOMET</param>
        /// <param name="tokenPayUser">user Token in PayCOMET</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse DeferredPreauthorizationConfirm(string idPayUser, string tokenPayUser, string ipClient, string amount, string transReference)
        {
            BankstoreServResponse result = new BankstoreServResponse();

            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + idPayUser + tokenPayUser + terminal + transReference + amount + password);

            try
            {
                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();
                string dsErrorId, dsMerchantAuthCode, dsMerchantCardCountry, dsResponse = string.Empty;

                string dsMerchantCurrency = wsProxy.deferred_preauthorization_confirm(merchantCode, terminal, idPayUser, tokenPayUser,
                    ref amount, ref transReference, signature, ipClient, out dsMerchantAuthCode, out dsMerchantCardCountry,
                    out dsResponse, out dsErrorId);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_MERCHANT_AMOUNT", amount);
                    result.Data.Add("DS_MERCHANT_ORDER", transReference);
                    result.Data.Add("DS_MERCHANT_CURRENCY", dsMerchantCurrency);
                    result.Data.Add("DS_MERCHANT_AUTHCODE", dsMerchantAuthCode);
                    result.Data.Add("DS_MERCHANT_CARDCOUNTRY", dsMerchantCardCountry);
                    result.Data.Add("DS_RESPONSE", dsResponse);

                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Cancels a deferred preauthorization by web service.
        /// </summary>
        /// <param name="idPayUser">User ID in PayCOMET</param>
        /// <param name="tokenPayUser">user Token in PayCOMET</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse DeferredPreauthorizationCancel(string idPayUser, string tokenPayUser, string ipClient, string amount, string transReference)
        {
            BankstoreServResponse result = new BankstoreServResponse();

            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + idPayUser + tokenPayUser + terminal + transReference + amount + password);

            try
            {
                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();
                string dsErrorId, dsMerchantAuthCode, dsMerchantCardCountry, dsResponse = string.Empty;

                string dsMerchantCurrency = wsProxy.deferred_preauthorization_cancel(merchantCode, terminal, idPayUser, tokenPayUser,
                    ref amount, ref transReference, signature, ipClient, out dsMerchantAuthCode, out dsMerchantCardCountry,
                    out dsResponse, out dsErrorId);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_MERCHANT_AMOUNT", amount);
                    result.Data.Add("DS_MERCHANT_ORDER", transReference);
                    result.Data.Add("DS_MERCHANT_CURRENCY", dsMerchantCurrency);
                    result.Data.Add("DS_MERCHANT_AUTHCODE", dsMerchantAuthCode);
                    result.Data.Add("DS_MERCHANT_CARDCOUNTRY", dsMerchantCardCountry);
                    result.Data.Add("DS_RESPONSE", dsResponse);

                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Add a user by using web service BankStore JET
        /// </summary>
        /// <param name="jetToken">jetToken temporary user Token in PayCOMET</param>
        /// <param name="ipClient">ip of the client performing the operation</param>
        /// <param name="jetId">jetId in configuration PayCOMET</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse AddUserToken(string jetToken, string ipClient, string jetId)
        {
            BankstoreServResponse result = new BankstoreServResponse();

            var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + jetToken + jetId + terminal + password);

            try
            {
                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();
                string dsErrorId, dsTokenUser = string.Empty;

                string dsIdUser = wsProxy.add_user_token(merchantCode, terminal, jetToken, jetId, signature, ipClient, out dsTokenUser, out dsErrorId);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_IDUSER", dsIdUser);
                    result.Data.Add("DS_TOKEN_USER", dsTokenUser);

                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Executes a payment for web service with the "payment by reference" for the migration to PayComet
        /// </summary>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="rToken">Original card reference stored in old system</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="productDescription">description Operation description</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse ExecutePurchaseRToken(string amount, string transReference, string rToken, string currency, string productDescription = null)
        {
            BankstoreServResponse result = new BankstoreServResponse();

            try
            {
                var signature = Cryptography.SHA512HashStringForUTF8String(merchantCode + terminal + amount + transReference + rToken + password);

                PAYTPV_BankStoreGatewayService wsProxy = new PAYTPV_BankStoreGatewayService();

                string dsErrorId, dsMerchantCardCountry, dsResponse = string.Empty;
                string dsMerchantDescriptor = string.Empty;
                string authCode = wsProxy.execute_purchase_rtoken(merchantCode, terminal, ref amount, ref transReference, rToken, ref currency, signature, productDescription, dsMerchantDescriptor, out dsMerchantCardCountry, out dsResponse, out dsErrorId);

                if (Convert.ToInt32(dsErrorId) > 0)
                {
                    result.DsErrorId = dsErrorId;
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data.Add("DS_MERCHANT_AMOUNT", amount);
                    result.Data.Add("DS_MERCHANT_ORDER", transReference);
                    result.Data.Add("DS_MERCHANT_CURRENCY", currency);
                    result.Data.Add("DS_MERCHANT_AUTHCODE", authCode);
                    result.Data.Add("DS_MERCHANT_CARDCOUNTRY", dsMerchantCardCountry);
                    result.Data.Add("DS_RESPONSE", dsResponse);

                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Returns the URL to launch a add_user under IFRAME / Fullscreen
        /// </summary>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="lang">language transaction literals</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse AddUserUrl(string transReference, string lang = "ES")
        {
            BankstoreServResponse result = new BankstoreServResponse();
            OperationData = new OperationData();
            try
            {
                OperationData.Type = OperationTypes.ADD_USER;
                OperationData.Reference = transReference;
                OperationData.Language = lang;
                OperationData.Hash = GenerateHash(OperationData.Type); //generate hash
                string lastRequest = ComposeURLParams(OperationData.Type);

                this.endpointUrl = this.endpoint;
                result = CheckUrlError(this.endpointUrl + lastRequest);
                if (Convert.ToInt32(result.DsErrorId) > 0)
                {
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data["URL_REDIRECT"] = this.endpointUrl + lastRequest;
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Returns the URL to launch a execute_purchase under IFRAME / Fullscreen
        /// </summary>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="lang">language transaction literals</param>
        /// <param name="description">description Operation description</param>
        /// <param name="secure3D">secure3D Force operation 0 = No 1 = Safe and secure by 3DSecure</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse ExecutePurchaseUrl(string transReference, string amount, string currency, string lang = "ES", string description = null, string secure3D = "0", string scoring = null)
        {
            BankstoreServResponse result = new BankstoreServResponse();
            OperationData = new OperationData();
            try
            {
                OperationData.Type = OperationTypes.EXECUTE_PURCHASE;
                OperationData.Reference = transReference;
                OperationData.Amount = amount;
                OperationData.Currency = currency;
                OperationData.Language = lang;
                OperationData.Concept = description;
                OperationData.Secure3D = secure3D;
                OperationData.Scoring = scoring;

                OperationData.Hash = GenerateHash(OperationData.Type); //generate hash
                string lastRequest = ComposeURLParams(OperationData.Type);

                result = CheckUrlError(this.endpointUrl + lastRequest);
                if (Convert.ToInt32(result.DsErrorId) > 0)
                {
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data["URL_REDIRECT"] = this.endpointUrl + lastRequest;
                    result.Result = "OK";
                }

            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }

            return result;
        }

        /// <summary>
        /// Returns the URL to launch a create_subscription under IFRAME / Fullscreen
        /// </summary>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="startDate">startDate date subscription start yyyy-mm-dd</param>
        /// <param name="endDate">endDate Date End subscription yyyy-mm-dd</param>
        /// <param name="periodicity">periodicity Frequency of subscription. In days.</param>
        /// <param name="lang">language transaction literals</param>
        /// <param name="secure3D">secure3D Force operation 0 = No 1 = Safe and secure by 3DSecure</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse CreateSubscriptionUrl(string transReference, string amount, string currency, string startDate, string endDate,
            string periodicity, string lang = "ES", string secure3D = "0", string scoring = null)
        {
            BankstoreServResponse result = new BankstoreServResponse();
            OperationData = new OperationData();
            try
            {
                OperationData.Type = OperationTypes.CREATE_SUBSCRIPTION;
                OperationData.Reference = transReference;
                OperationData.Amount = amount;
                OperationData.Currency = currency;
                OperationData.Language = lang;
                OperationData.Periodicity = periodicity;
                OperationData.StartDate = startDate;
                OperationData.EndDate = endDate;
                OperationData.Secure3D = secure3D;
                OperationData.Scoring = scoring;

                OperationData.Hash = GenerateHash( OperationData.Type); //generate hash
                string lastRequest = ComposeURLParams( OperationData.Type);
                string urlRedirect = this.endpointUrl + lastRequest;
                result = CheckUrlError(urlRedirect);
                if (Convert.ToInt32(result.DsErrorId) > 0)
                {
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data["URL_REDIRECT"] = this.endpointUrl + lastRequest;
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Returns the URL to launch a execute_purchase_token under IFRAME / Fullscreen
        /// </summary>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="idUser">idUser unique identifier system registered user.</param>
        /// <param name="tokenUser">tokenUser token code associated to IDUSER.</param>
        /// <param name="lang">language transaction literals</param>
        /// <param name="description">description Operation description</param>
        /// <param name="secure3D">secure3D Force operation 0 = No 1 = Safe and secure by 3DSecure</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse ExecutePurchaseTokenUrl(string transReference, string amount, string currency, string idUser, string tokenUser,
            string lang = "ES", string description = null, string secure3D = "1", string scoring = null)
        {
            BankstoreServResponse result = new BankstoreServResponse();
            OperationData = new OperationData();
            try
            {
                OperationData.Type = OperationTypes.EXECUTE_PURCHASE_TOKEN;
                OperationData.Reference = transReference;
                OperationData.Amount = amount;
                OperationData.Currency = currency;
                OperationData.Language = lang;
                OperationData.Concept = description;
                OperationData.IdUser = idUser;
                OperationData.TokenUser = tokenUser;
                OperationData.Secure3D = secure3D;
                OperationData.Scoring = scoring;

                OperationData.Hash = GenerateHash( OperationData.Type); //generate hash
                string lastRequest = ComposeURLParams( OperationData.Type);
                string urlRedirect = this.endpointUrl + lastRequest;
                result = CheckUrlError(urlRedirect);

                if (Convert.ToInt32(result.DsErrorId) > 0)
                {
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data["URL_REDIRECT"] = this.endpointUrl + lastRequest;
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Returns the URL to launch a create_subscription_token under IFRAME / Fullscreen
        /// </summary>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="startDate">startDate date subscription start yyyy-mm-dd</param>
        /// <param name="endDate">endDate Date End subscription yyyy-mm-dd</param>
        /// <param name="periodicity">periodicity Frequency of subscription. In days.</param>
        /// <param name="idUser">idUser unique identifier system registered user.</param>
        /// <param name="tokenUser">tokenUser token code associated to IDUSER.</param>
        /// <param name="lang">language transaction literals</param>
        /// <param name="secure3D">secure3D Force operation 0 = No 1 = Safe and secure by 3DSecure</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse CreateSubscriptionTokenUrl(string transReference, string amount, string currency, string startDate, string endDate,
            string periodicity, string idUser, string tokenUser, string lang = "ES", string secure3D = "0", string scoring = null)
        {
            BankstoreServResponse result = new BankstoreServResponse();
            OperationData = new OperationData();
            try
            {
                OperationData.Type = OperationTypes.CREATE_SUBSCRIPTION_TOKEN;
                OperationData.Reference = transReference;
                OperationData.Amount = amount;
                OperationData.Currency = currency;
                OperationData.Language = lang;
                OperationData.Periodicity = periodicity;
                OperationData.StartDate = startDate;
                OperationData.EndDate = endDate;
                OperationData.IdUser = idUser;
                OperationData.TokenUser = tokenUser;
                OperationData.Secure3D = secure3D;
                OperationData.Scoring = scoring;

                OperationData.Hash = GenerateHash(OperationData.Type); //generate hash
                string lastRequest = ComposeURLParams(OperationData.Type);
                string urlRedirect = this.endpointUrl + lastRequest;
                result = CheckUrlError(urlRedirect);

                if (Convert.ToInt32(result.DsErrorId) > 0)
                {
                    result.Result = "KO";

                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data["URL_REDIRECT"] = this.endpointUrl + lastRequest;
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Returns the URL to launch a create_preauthorization under IFRAME / Fullscreen
        /// </summary>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="lang">language transaction literals</param>
        /// <param name="description">description Operation description</param>
        /// <param name="secure3D">secure3D Force operation 0 = No 1 = Safe and secure by 3DSecure</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse CreatePreauthorizationUrl(string transReference, string amount, string currency, string lang = "ES", string description = null, string secure3D = "0", string scoring = null)
        {
            BankstoreServResponse result = new BankstoreServResponse();
            OperationData = new OperationData();
            try
            {
                OperationData.Type = OperationTypes.CREATE_PREAUTHORIZATION;
                OperationData.Reference = transReference;
                OperationData.Amount = amount;
                OperationData.Currency = currency;
                OperationData.Language = lang;
                OperationData.Concept = description;
                OperationData.Secure3D = secure3D;
                OperationData.Scoring = scoring;

                OperationData.Hash = GenerateHash(OperationData.Type); //generate hash
                string lastRequest = ComposeURLParams(OperationData.Type);
                string urlRedirect = this.endpointUrl + lastRequest;
                result = CheckUrlError(urlRedirect);

                if (Convert.ToInt32(result.DsErrorId) > 0)
                {
                    result.Result = "KO";

                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data["URL_REDIRECT"] = this.endpointUrl + lastRequest;
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Returns the URL to launch a preauthorization_confirm under IFRAME / Fullscreen
        /// </summary>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="idUser">idUser unique identifier system registered user.</param>
        /// <param name="tokenUser">tokenUser token code associated to IDUSER.</param>
        /// <param name="lang">language transaction literals</param>
        /// <param name="description">description Operation description</param>
        /// <param name="secure3D">secure3D Force operation 0 = No 1 = Safe and secure by 3DSecure</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse PreauthorizationConfirmUrl(string IpClient, string transReference, string amount, string currency, string idUser,
                                                                string tokenUser, string lang = "ES", string description = null, string secure3D = "0")
        {
            BankstoreServResponse result = new BankstoreServResponse();
            OperationData = new OperationData();
            try
            {
                OperationData.Type = OperationTypes.PREAUTHORIZATION_CONFIRM;
                OperationData.Reference = transReference;
                OperationData.Amount = amount;
                OperationData.Currency = currency;
                OperationData.Language = lang;
                OperationData.Concept = description;
                OperationData.IdUser = idUser;
                OperationData.TokenUser = tokenUser;
                OperationData.Secure3D = secure3D;

                var checkUserExist = InfoUser(OperationData.IdUser, OperationData.TokenUser, IpClient);
                if (Convert.ToInt32(checkUserExist.DsErrorId) > 0)
                {
                    return checkUserExist;
                }
                OperationData.Hash = GenerateHash(OperationData.Type); //generate hash
                string lastRequest = ComposeURLParams(OperationData.Type);
                string urlRedirect = this.endpointUrl + lastRequest;
                result = CheckUrlError(urlRedirect);

                if (Convert.ToInt32(result.DsErrorId) > 0)
                {
                    result.Result = "KO";

                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data["URL_REDIRECT"] = this.endpointUrl + lastRequest;
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Returns the URL to launch a preauthorization_cancel under IFRAME / Fullscreen
        /// </summary>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="idUser">idUser unique identifier system registered user.</param>
        /// <param name="tokenUser">tokenUser token code associated to IDUSER.</param>
        /// <param name="lang">language transaction literals</param>
        /// <param name="description">description Operation description</param>
        /// <param name="secure3D">secure3D Force operation 0 = No 1 = Safe and secure by 3DSecure</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse PreauthorizationCancelUrl(string IpClient, string transReference, string amount, string currency, string idUser,
                                                               string tokenUser, string lang = "ES", string description = null, string secure3D = "0")
        {
            BankstoreServResponse result = new BankstoreServResponse();
            OperationData = new OperationData();
            try
            {
                OperationData.Type = OperationTypes.PREAUTHORIZATION_CANCEL;
                OperationData.Reference = transReference;
                OperationData.Amount = amount;
                OperationData.Currency = currency;
                OperationData.Language = lang;
                OperationData.Concept = description;
                OperationData.IdUser = idUser;
                OperationData.TokenUser = tokenUser;
                OperationData.Secure3D = secure3D;

                var checkUserExist = InfoUser(OperationData.IdUser, OperationData.TokenUser, IpClient);
                if (Convert.ToInt32(checkUserExist.DsErrorId) > 0)
                {
                    return checkUserExist;
                }
                OperationData.Hash = GenerateHash(OperationData.Type); //generate hash
                string lastRequest = ComposeURLParams(OperationData.Type);
                string urlRedirect = this.endpointUrl + lastRequest;
                result = CheckUrlError(urlRedirect);

                if (Convert.ToInt32(result.DsErrorId) > 0)
                {
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data["URL_REDIRECT"] = this.endpointUrl + lastRequest;
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Returns the URL to launch a execute_preauthorization_token under IFRAME / Fullscreen
        /// </summary>
        /// <param name="transReference">transReference unique identifier payment</param>\
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="idUser">idUser unique identifier system registered user.</param>
        /// <param name="tokenUser">tokenUser token code associated to IDUSER.</param>
        /// <param name="lang">language transaction literals</param>
        /// <param name="description">description Operation description</param>
        /// <param name="secure3D">secure3D Force operation 0 = No 1 = Safe and secure by 3DSecure</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse CreatePreauthorizationTokenUrl(string IpClient, string transReference, string amount, string currency, string idUser,
                                                                    string tokenUser, string lang = "ES", string description = null, string secure3D = "0", string scoring = null)
        {
            BankstoreServResponse result = new BankstoreServResponse();
            OperationData = new OperationData();
            try
            {
                OperationData.Type = OperationTypes.CREATE_PREAUTHORIZATION_TOKEN;
                OperationData.Reference = transReference;
                OperationData.Amount = amount;
                OperationData.Currency = currency;
                OperationData.Language = lang;
                OperationData.Concept = description;
                OperationData.IdUser = idUser;
                OperationData.TokenUser = tokenUser;
                OperationData.Secure3D = secure3D;
                OperationData.Scoring = scoring;

                var checkUserExist = InfoUser(OperationData.IdUser, OperationData.TokenUser, IpClient);
                if (Convert.ToInt32(checkUserExist.DsErrorId) > 0)
                {
                    return checkUserExist;
                }
                OperationData.Hash = GenerateHash(OperationData.Type); //generate hash
                string lastRequest = ComposeURLParams(OperationData.Type);
                string urlRedirect = this.endpointUrl + lastRequest;
                result = CheckUrlError(urlRedirect);

                if (Convert.ToInt32(result.DsErrorId) > 0)
                {
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data["URL_REDIRECT"] = this.endpointUrl + lastRequest;
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Returns the URL to launch a deferred_preauthorization under IFRAME / Fullscreen
        /// </summary>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="lang">language transaction literals</param>
        /// <param name="description">description Operation description</param>
        /// <param name="secure3D">secure3D Force operation 0 = No 1 = Safe and secure by 3DSecure</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse DeferredPreauthorizationUrl(string transReference, string amount, string currency, string lang = "ES", string description = null, string secure3D = "0", string scoring = null)
        {
            BankstoreServResponse result = new BankstoreServResponse();
            OperationData = new OperationData();
            try
            {
                OperationData.Type = OperationTypes.DEFERRED_CREATE_PREAUTHORIZATION;
                OperationData.Reference = transReference;
                OperationData.Amount = amount;
                OperationData.Currency = currency;
                OperationData.Language = lang;
                OperationData.Concept = description;
                OperationData.Secure3D = secure3D;
                OperationData.Scoring = scoring;

                OperationData.Hash = GenerateHash(OperationData.Type); //generate hash
                string lastRequest = ComposeURLParams(OperationData.Type);
                string urlRedirect = this.endpointUrl + lastRequest;
                result = CheckUrlError(urlRedirect);

                if (Convert.ToInt32(result.DsErrorId) > 0)
                {
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data["URL_REDIRECT"] = this.endpointUrl + lastRequest;
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Returns the URL to launch a deferred_preauthorization_confirm under IFRAME / Fullscreen
        /// </summary>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="idUser">idUser unique identifier system registered user.</param>
        /// <param name="tokenUser">tokenUser token code associated to IDUSER.</param>
        /// <param name="lang">language transaction literals</param>
        /// <param name="description">description Operation description</param>
        /// <param name="secure3D">secure3D Force operation 0 = No 1 = Safe and secure by 3DSecure</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse DeferredPreauthorizationConfirmUrl(string IpClient, string transReference, string amount, string currency, string idUser, string tokenUser, string lang = "ES", string description = null, string secure3D = "0")
        {
            BankstoreServResponse result = new BankstoreServResponse();
            OperationData = new OperationData();
            try
            {
                OperationData.Type = OperationTypes.DEFERRED_PREAUTHORIZATION_CONFIRM;
                OperationData.Reference = transReference;
                OperationData.Amount = amount;
                OperationData.Currency = currency;
                OperationData.Language = lang;
                OperationData.Concept = description;
                OperationData.IdUser = idUser;
                OperationData.TokenUser = tokenUser;
                OperationData.Secure3D = secure3D;

                var checkUserExist = InfoUser(OperationData.IdUser, OperationData.TokenUser, IpClient);
                if (Convert.ToInt32(checkUserExist.DsErrorId) > 0)
                {
                    return checkUserExist;
                }
                OperationData.Hash = GenerateHash(OperationData.Type); //generate hash
                string lastRequest = ComposeURLParams(OperationData.Type);
                string urlRedirect = this.endpointUrl + lastRequest;
                result = CheckUrlError(urlRedirect);

                if (Convert.ToInt32(result.DsErrorId) > 0)
                {
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data["URL_REDIRECT"] = this.endpointUrl + lastRequest;
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }

        /// <summary>
        /// Returns the URL to launch a deferred_preauthorization_cancel under IFRAME / Fullscreen
        /// </summary>
        /// <param name="transReference">transReference unique identifier payment</param>
        /// <param name="amount">Amount of payment 1 € = 100</param>
        /// <param name="currency">currency identifier transaction currency</param>
        /// <param name="idUser">idUser unique identifier system registered user.</param>
        /// <param name="tokenUser">tokenUser token code associated to IDUSER.</param>
        /// <param name="lang">language transaction literals</param>
        /// <param name="description">description Operation description</param>
        /// <param name="secure3D">secure3D Force operation 0 = No 1 = Safe and secure by 3DSecure</param>
        /// <returns>transaction response</returns>
        public BankstoreServResponse DeferredPreauthorizationCancelUrl(string IpClient, string transReference, string amount, string currency, string idUser, string tokenUser, string lang = "ES", string description = null, string secure3D = "0")
        {
            BankstoreServResponse result = new BankstoreServResponse();
            OperationData = new OperationData();
            try
            {
                OperationData.Type = OperationTypes.DEFERRED_PREAUTHORIZATION_CANCEL;
                OperationData.Reference = transReference;
                OperationData.Amount = amount;
                OperationData.Currency = currency;
                OperationData.Language = lang;
                OperationData.Concept = description;
                OperationData.IdUser = idUser;
                OperationData.TokenUser = tokenUser;
                OperationData.Secure3D = secure3D;

                var checkUserExist = InfoUser(OperationData.IdUser, OperationData.TokenUser, IpClient);
                if (Convert.ToInt32(checkUserExist.DsErrorId) > 0)
                {
                    return checkUserExist;
                }
                OperationData.Hash = GenerateHash(OperationData.Type); //generate hash
                string lastRequest = ComposeURLParams(OperationData.Type);
                string urlRedirect = this.endpointUrl + lastRequest;
                result = CheckUrlError(urlRedirect);

                if (Convert.ToInt32(result.DsErrorId) > 0)
                {
                    result.Result = "KO";
                }
                else
                {
                    result.Data = new Dictionary<string, string>();
                    result.Data["URL_REDIRECT"] = this.endpointUrl + lastRequest;
                    result.Result = "OK";
                }
            }
            catch (HttpException)
            {
                result.DsErrorId = "1011";
                result.Result = "KO";
            }
            catch (Exception)
            {
                result.DsErrorId = "1002";
                result.Result = "KO";
            }
            return result;
        }


        private string GenerateHash(int operationType)
        {
            string hash = string.Empty;
            using (MD5 md5Hash = MD5.Create())
            {
                switch (operationType)
                {
                    case OperationTypes.EXECUTE_PURCHASE:
                    case OperationTypes.CREATE_PREAUTHORIZATION:
                    case OperationTypes.CREATE_SUBSCRIPTION:
                    case OperationTypes.DEFERRED_CREATE_PREAUTHORIZATION:
                        hash = Cryptography.ComputeMD5(md5Hash, merchantCode + terminal + operationType + OperationData.Reference + OperationData.Amount +
                        OperationData.Currency + Cryptography.ComputeMD5(md5Hash, password));
                        break;
                    case OperationTypes.PREAUTHORIZATION_CONFIRM:
                    case OperationTypes.PREAUTHORIZATION_CANCEL:
                    case OperationTypes.DEFERRED_PREAUTHORIZATION_CONFIRM:
                    case OperationTypes.DEFERRED_PREAUTHORIZATION_CANCEL:
                        hash = Cryptography.ComputeMD5(md5Hash, merchantCode + OperationData.IdUser + OperationData.TokenUser + terminal + operationType + OperationData.Reference + OperationData.Amount +
                        Cryptography.ComputeMD5(md5Hash, password));
                        break;
                    case OperationTypes.ADD_USER:
                        hash = Cryptography.ComputeMD5(md5Hash, merchantCode + terminal + operationType + OperationData.Reference + Cryptography.ComputeMD5(md5Hash, password));
                        break;
                    case OperationTypes.EXECUTE_PURCHASE_TOKEN:
                    case OperationTypes.CREATE_SUBSCRIPTION_TOKEN:
                    case OperationTypes.CREATE_PREAUTHORIZATION_TOKEN:
                        hash = Cryptography.ComputeMD5(md5Hash, merchantCode + OperationData.IdUser + OperationData.TokenUser + terminal + operationType + OperationData.Reference + OperationData.Amount +
                         OperationData.Currency + Cryptography.ComputeMD5(md5Hash, password));
                        break;
                    default:
                        break;
                }
            }
            return hash;
        }

        private string ComposeURLParams(int operationType)
        {
            string secureUrlHash = string.Empty;
            SortedDictionary<string, string> data = new SortedDictionary<string, string>();

            data["MERCHANT_MERCHANTCODE"] = merchantCode;
            data["MERCHANT_TERMINAL"] = terminal;
            data["OPERATION"] = operationType.ToString();
            data["LANGUAGE"] = OperationData.Language;
            data["MERCHANT_MERCHANTSIGNATURE"] = OperationData.Hash;
            data["URLOK"] = OperationData.UrlOk;
            data["URLKO"] = OperationData.UrlKo;
            data["MERCHANT_ORDER"] = OperationData.Reference;
            data["3DSECURE"] = OperationData.Secure3D;
            data["MERCHANT_AMOUNT"] = OperationData.Amount;
            if (!string.IsNullOrEmpty(OperationData.Concept))
                data["MERCHANT_PRODUCTDESCRIPTION"] = OperationData.Concept;

            switch (operationType)
            {
                case OperationTypes.EXECUTE_PURCHASE:
                case OperationTypes.CREATE_PREAUTHORIZATION:
                case OperationTypes.DEFERRED_CREATE_PREAUTHORIZATION:
                    data["MERCHANT_CURRENCY"] = OperationData.Currency;
                    if (!string.IsNullOrEmpty(OperationData.Scoring))
                        data["MERCHANT_SCORING"] = OperationData.Scoring;
                    break;
                case OperationTypes.PREAUTHORIZATION_CONFIRM:
                case OperationTypes.PREAUTHORIZATION_CANCEL:
                case OperationTypes.DEFERRED_PREAUTHORIZATION_CONFIRM:
                case OperationTypes.DEFERRED_PREAUTHORIZATION_CANCEL:
                    data["MERCHANT_CURRENCY"] = OperationData.Currency;
                    data["IDUSER"] = OperationData.IdUser;
                    data["TOKEN_USER"] = OperationData.TokenUser;
                    break;
                case OperationTypes.CREATE_SUBSCRIPTION:
                    data["MERCHANT_CURRENCY"] = OperationData.Currency;
                    data["SUBSCRIPTION_STARTDATE"] = OperationData.StartDate;
                    data["SUBSCRIPTION_ENDDATE"] = OperationData.EndDate;
                    data["SUBSCRIPTION_PERIODICITY"] = OperationData.Periodicity;
                    if (!string.IsNullOrEmpty(OperationData.Scoring))
                        data["MERCHANT_SCORING"] = OperationData.Scoring;
                    break;
                case OperationTypes.EXECUTE_PURCHASE_TOKEN:
                case OperationTypes.CREATE_PREAUTHORIZATION_TOKEN:
                    data["IDUSER"] = OperationData.IdUser;
                    data["TOKEN_USER"] = OperationData.TokenUser;
                    data["MERCHANT_CURRENCY"] = OperationData.Currency;
                    if (!string.IsNullOrEmpty(OperationData.Scoring))
                        data["MERCHANT_SCORING"] = OperationData.Scoring;
                    break;
                case OperationTypes.CREATE_SUBSCRIPTION_TOKEN:
                    data["IDUSER"] = OperationData.IdUser;
                    data["TOKEN_USER"] = OperationData.TokenUser;
                    data["MERCHANT_CURRENCY"] = OperationData.Currency;
                    data["SUBSCRIPTION_STARTDATE"] = OperationData.StartDate;
                    data["SUBSCRIPTION_ENDDATE"] = OperationData.EndDate;
                    data["SUBSCRIPTION_PERIODICITY"] = OperationData.Periodicity;
                    if (!string.IsNullOrEmpty(OperationData.Scoring))
                        data["MERCHANT_SCORING"] = OperationData.Scoring;
                    break;
                default:
                    break;
            }

            string content = string.Empty;
            foreach (var k in data.Keys)
                content += "&" + string.Format("{0}={1}", HttpUtility.UrlEncode(k), HttpUtility.UrlEncode(data[k]));
            content = content.Remove(0, 1);
            using (MD5 md5Hash = MD5.Create())
            {
                data["VHASH"] = Cryptography.SHA512HashStringForUTF8String(Cryptography.ComputeMD5(md5Hash, content + Cryptography.ComputeMD5(md5Hash, password)));
            }
            foreach (var k in data.Keys)
                secureUrlHash += "&" + string.Format("{0}={1}", HttpUtility.UrlEncode(k), HttpUtility.UrlEncode(data[k]));
            secureUrlHash = secureUrlHash.Remove(0, 1);
            return secureUrlHash;
        }

        private BankstoreServResponse CheckUrlError(string urlGen)
        {
            BankstoreServResponse response = new BankstoreServResponse();
            using (WebClient webClient = new WebClient())
            {
                try
                {
                    string data = webClient.DownloadString(urlGen);
                    if (Regex.IsMatch(data, @"Error: \d+"))
                    {
                        response.DsErrorId = Regex.Match(data, @"\d+").Value;
                        response.Result = "KO";
                    }
                    else
                    {
                        //response.Data = new Dictionary<string, string>();
                        //response.Data.Add("data", data);
                        response.Result = "OK";
                    }
                }
                catch (HttpException)
                {
                    response.DsErrorId = "1011";
                    response.Result = "KO";
                }
                catch (Exception)
                {
                    response.DsErrorId = "1002";
                    response.Result = "KO";
                }
            }
            return response;
        }

    }
}
