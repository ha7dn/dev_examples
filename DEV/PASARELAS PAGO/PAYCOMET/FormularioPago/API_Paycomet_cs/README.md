### API de PAYCOMET BankStore en ASP NET y C#

Este es el API de conexión con todos los servicios de PAYCOMET BankStore mediante XML, IFRAME, FULLSCREEN y JET.

### Instalación

Descarga el proyecto con la solución **API_Paycomet_cs**

Dentro de la misma encontrarás:

 - **API_Paycomet_cs**:  API con los servicios Paycomet de las integraciones Bankstore XML - IFRAME - JET IFRAME
 - **1 - Desktop_Client_cs**: La aplicacion cliente de escritorio, que contiene ejemplos de uso con llamadas a API (API_Paycomet_cs)
 - **2-Web_Client_cs**: La aplicacion web, que contiene dos ejemplos de compras: usuarios corrientes (se puede aplicar para ambos casos) o usuarios PCI DSS.

Para realizar pruebas con cualquira de las aplicaciones, **Desktop_Client_cs** o **Web_Client_cs** , no te olvides de establecer ese proyecto como *"Proyecto de inicio"** haciendo click derecho sobre el mismo y seleccionado la opción antes de ejecutar la solución.

### Aplicación "API_Paycomet_cs"
No es necesario alterar su funcionamiento. El listado de **métodos**(*) disponibles se encuentra al final del documento.

## 1 - Aplicación "Desktop_Client_cs"
Esto es solo un ejemplo de integración con una aplicación de consola.

Dentro de esta aplicación se encuentra el archivo **Program.cs**, tendrás que configurar las variables con los datos de tu termial obtenidos en `https://dashboard.paycomet.com/cp_control/index.php` en el menú **Mis Productos -> Configurar productos -> Editar**
```sh
MerchantCode => Corresponde al Código de cliente
Terminal => Correpsonde al Número de terminal
Password => Corresponde a la Contraseña
ipClient => Es la Ip del Cliente(final) que realizará las peticiones
endpoint => "https://api.paycomet.com/gateway/xml-bankstore?wsdl"
endpointUrl => "https://api.paycomet.com/gateway/ifr-bankstore?"
```

Con la configuración anterior, ya podrás crear un objeto de tipo **Paycomet_Bankstore**

```sh
Paycomet_Bankstore bs = new Paycomet_Bankstore(MerchantCode, Terminal, Password, endpoint, endpointUrl);
```

También tendrás que añadir los datos de tu tarjeta para que pueda ser tokenizada, los datos son los siguientes:
```sh
pan => Corresponde al número de la tarjeta
expDate = Correponde a la fecha de caducidad de la tarjeta
cvv => Correponde al numero de seguridad de la tarjeta
```

De esta forma, ya podremos hacer llamadas a los métodos de la API, como por ejemplo:

```sh
BankstoreServResponse add_user = bs.AddUser(pan, expDate, cvv, ipClient);
```

## 2 - Aplicación "Web_Client_cs"
Es posible que tengas que actualizar los paquetes NuGet del proyecto si al ejecutar el mismo recibes algún tipo de error, en caso contrario omite este punto.

Para poder realizar uso, debes modificar las variables de configuración dentro del archivo **Web.config**,  con los datos de tu termial obtenidos en `https://dashboard.paycomet.com/cp_control/index.php` en el menú **Mis Productos -> Configurar productos -> Editar**
```sh
MerchantCode => Corresponde al Código de cliente
Terminal => Correpsonde al Número de terminal
Password => Corresponde a la Contraseña
JetId => Se genera dando al botón que aparece al lado del mismo, dentro del panel de administración de tu roducto, y se obtiene una cadena alfanumérica
endpoint => "https://api.paycomet.com/gateway/xml-bankstore?wsdl"
endpointUrl => "https://api.paycomet.com/gateway/ifr-bankstore?"
```

El proyecto dispone de un Controlador: **HomeController** y tres vistas: **Home, Form1, Form2**. Estas dos últimas son ejemplos de implementación de un proceso de compra.

* Implementación **Form1**, realizada usando JET-IFRAME  'https://docs.paycomet.com/es/documentacion/bankstore_jetiframe'
* Implementación **Form2**, usando el método de la clase API_Paycomet_cs ExecutePurchase

**La integración de JET-IFRAME es la recomendada** ya que la podrá realizar cualquier comercio, en esta integración todos los datos de tarjeta se procesan en los **servidores seguros** de PAYCOMET y no tiene que preocuparse de realizar más que las llamadas.


## (*) Integración de métodos API_Paycomet_cs

| Método | Integración | Descripción |
| ------ | ------ | ------ |
| AddUser | BankStore XML | Ejecución de alta de usuario en el sistema |
| InfoUser | BankStore XML | Información del usuario |
| RemoveUser |  BankStore XML | Eliminación del usuario |
| ExecutePurchase |  BankStore XML | Ejecución de cobro a Usuario en el sistema |
| ExecutePurchaseDcc |  BankStore XML | Ejecución de cobro a Usuario en el sistema por DCC |
| ConfirmPurchaseDcc |  BankStore XML | Confirmación de moneda en pago DCC |
| ExecuteRefund |  BankStore XML | Devolución de cobro a usuario en el sistema |
| CreateSubscription |  BankStore XML | Ejecución de alta de suscripción en el sistema |
| EditSubscription |  BankStore XML | Modificación de suscripción en el sistema|
| RemoveSubscription | BankStore XML | Eliminación de Suscripción |
| CreateSubscriptionToken | BankStore XML |  Ejecución de alta de suscripción en el sistema con USERID Y TOKENID |
| CreatePreauthorization |  BankStore XML | Creación de una preautorización a usuario en el sistema |
| PreauthorizationConfirm |  BankStore XML | Confirmación de una preautorización a usuario en el sistema |
| PreauthorizationCancel |  BankStore XML | Cancelación de una preautorización a usuario en el sistema |
| DeferredPreauthorizationConfirm |  BankStore XML | Confirmación de una preautorización diferida a usuario en el sistema |
| DeferredPreauthorizationCancel |  BankStore XML | Cancelación de una preautorización diferida a usuario en el sistema |
| AddUserToken | BankStore XML |  Ejecución de alta de usuario en el sistema mediante Token |
| ExecutePurchaseRToken |  BankStore XML | Ejecución de Cobro a un usuario por Referencia |
| AddUserUrl | BankStore IFRAME/XML | Devuelve la url para iniciar una ejecuación de Alta de Usuario en el sistema |
| ExecutePurchaseUrl |  BankStore IFRAME/XML | Devuelve la url para iniciar una ejecución de cobro en el sistema (Alta implícita de Usuario en el sistema) |
| CreateSubscriptionUrl |  BankStore IFRAME/XML | Devuelve la url para iniciar un de alta de suscripción en el sistema (Alta implícita de Usuario en el sistema)|
| ExecutePurchaseTokenUrl |  BankStore IFRAME/XML | Devuelve la url para iniciar una ejecución de cobro existente|
| CreateSubscriptionTokenUrl |  BankStore IFRAME/XML | Devuelve la url para iniciar una ejecución de Alta de Suscripción a un usuario existente |
| CreatePreauthorizationUrl |  BankStore IFRAME/XML | Devuelve la url para iniciar una ejecución de Alta de Preautorización (Alta Implícita de Usuario en el sistema) |
| PreauthorizationConfirmUrl |  BankStore IFRAME/XML | Devuelve la url para iniciar una ejecución de Confirmación de Preautorización |
| PreauthorizationCancelUrl |  BankStore IFRAME/XML | Devuelve la url para iniciar una ejecución de Cancelación de Preautorización |
| CreatePreauthorizationTokenUrl |  BankStore IFRAME/XML | Devuelve la url para iniciar un alta de Preautorización a un usuario existente |
| DeferredPreauthorizationUrl |  BankStore IFRAME/XML | evuelve la url para iniciar una ejecución de alta de preautorización diferida (alta implícita de usuario en el sistema)|
| DeferredPreauthorizationConfirmUrl |  BankStore IFRAME/XML | Devuelve la url para iniciar una ejecución de Confirmación de Preautorización Diferida |
| DeferredPreauthorizationCancelUrl |  BankStore IFRAME/XML | Devuelve la url para iniciar una ejecución de Cancelación de Preautorización Diferida |

### Documentación

Enlace a la documentación: `https://docs.paycomet.com/es/documentacion/introduccion`

### Soporte
Si tienes alguna duda o pregunta puedes escribirnos un email a [tecnico@paycomet.com]

License
----

Paycomet
