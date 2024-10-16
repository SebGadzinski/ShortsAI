import React from "react";
import { Container } from "@mui/material";
import { makeStyles } from "@mui/styles";

const useStyles = makeStyles((theme) => ({
  container: {
    margin: "20px auto",
    width: "80%"
  }
}));

const TermsOfService = () => {
  const classes = useStyles();
  const websiteName = "Shorts AI";
  const companyName = "Gadzy Software & Consulting";
  const email = "shorts.@gmail.com";

  return (
    <Container className={classes.container}>
      <h1>Terms of Service</h1>
      <p>
        1. Introduction Welcome to {websiteName}. This website is operated by{" "}
        {companyName}. By visiting our website and accessing the information,
        resources, services, products, and tools we provide, you understand and
        agree to accept and adhere to the following terms and conditions.
      </p>
      <p>
        2. Service Description {websiteName} allows users to purchase credits,
        which can then be used to create videos.
      </p>
      <p>
        3. User Responsibilities Users are responsible for the content they
        submit to create videos. Users must ensure that they have the necessary
        rights to the content they submit.
      </p>
      <p>
        4. Intellectual Property The videos created on our website are generated
        by combining text and images from AI models. These videos can be used
        freely by the user without any strings attached to our website.
      </p>
      <p>
        5. Payments and Refunds: Users can purchase credits through Stripe, our
        secure payment processing partner. Stripe allows users to make payments
        using various payment methods, including credit cards, debit cards, and
        other supported payment options. Once credits are purchased, they are
        non-refundable and must be used on our website.
      </p>
      <p>
        6. Changes to Terms We reserve the right to modify these Terms at any
        time, so please review it frequently. Changes and clarifications will
        take effect immediately upon their posting on the website.
      </p>
      <p>
        7. Contact If you have any questions about our Terms of Service, please
        contact us via email: {email}.
      </p>
      <h1>Privacy Policy</h1>
      <p>
        1. Introduction At {companyName}, we respect the privacy of our users.
        This Privacy Policy explains how we collect, use, disclose, and
        safeguard your information when you visit our website.
      </p>
      <p>
        2. Collection and Use of Personal Data We collect information from you
        when you register on our site, place an order or fill out a form. Any
        data we request that is not required will be specified as voluntary or
        optional. When ordering or registering on our site, as appropriate, you
        may be asked to enter your: name, email address, mailing address, phone
        number, or credit card information.
      </p>
      <p>
        3. Cookies We use cookies to understand and save your preferences for
        future visits and compile aggregate data about site traffic and site
        interaction.
      </p>
      <p>
        4. Disclosure of Information We do not sell, trade, or otherwise
        transfer to outside parties your personally identifiable information.
      </p>
      <p>
        5. Security We implement a variety of security measures to maintain the
        safety of your personal information when you submit a request or access
        your personal information.
      </p>
      <p>
        6. Changes to Privacy Policy We reserve the right to modify this Privacy
        Policy at any time, so please review it frequently. Changes and
        clarifications will take effect immediately upon their posting on the
        website.
      </p>
      <p>
        7. Contact If you have any questions about our Privacy Policy, please
        contact us via email: {email}.
      </p>
    </Container>
  );
};

export default TermsOfService;
