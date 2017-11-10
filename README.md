# Stripe Checkout payment gateway module
Stripe Checkout payment  module provides integration with <a href="https://www.stripe.com" target="_blank">Stripe</a> payment api. 

# Installation
Installing the module:
* Automatically: in VC Manager go to Configuration -> Modules -> Stripe Checkout payment gateway module -> Install
* Manually: download module zip package from https://github.com/VirtoCommerce/vc-module-stripe/releases. In VC Manager go to Configuration -> Modules -> Advanced -> upload module package -> Install.

# Settings
* **Test Publishable key** - Stripe Account test publishable key
* **Test Secret key** - Stripe Account test secret key
* **Live Publishable key** - Stripe Account live publishable key
* **Live Secret key** - Stripe Account live secret key 
* **Working mode** - Test or live working mode
* **Process Payment action** - If the Stripe token successfuly created redirect to this URL to process payment. Default is {storefront URL}/cart/externalpaymentcallback

# License
Copyright (c) Virto Solutions LTD.  All rights reserved.

Licensed under the Virto Commerce Open Software License (the "License"); you
may not use this file except in compliance with the License. You may
obtain a copy of the License at

http://virtocommerce.com/opensourcelicense

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
implied.
