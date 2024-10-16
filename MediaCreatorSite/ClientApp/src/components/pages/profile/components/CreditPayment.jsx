import React, { useState } from "react";
import {
  Button,
  Container,
  FormControl,
  InputLabel,
  Select,
  MenuItem,
  Paper,
  Typography
} from "@mui/material";
import { _database } from "../../../../services/databaseService";
import { _notify } from "../../../../services/notifyService";
import { makeStyles } from "@mui/styles";
import localStorageService from "../../../../services/localStorageService";
import { loadStripe } from "@stripe/stripe-js";
import {
  Elements,
  CardElement,
  useStripe,
  useElements,
  CardNumberElement,
  CardExpiryElement,
  CardCvcElement
} from "@stripe/react-stripe-js";

const useStyles = makeStyles((theme) => ({
  container: {
    display: "flex !important",
    flexDirection: "column",
    width: "100%",
    margin: "0px auto !important",
    padding: "0px !important",
    justifyContent: "center"
  },
  cardField: {
    padding: theme.spacing(2) // Change the numeric value as per your requirements
  },
  paper: {
    margin: "10px 0px !important",
    padding: theme.spacing(1),
    textAlign: "center",
    alignContent: "center",
    alignItems: "center",
    color: theme.palette.text.secondary
  },
  formControl: {
    width: "100%"
  },
  selectEmpty: {
    marginTop: theme.spacing(2)
  },
  button: {
    margin: "10px auto !important"
  },
  textField: {
    margin: "10px auto !important"
  },
  payment_form: {
    margin: "10px 0px !important"
  }
}));

const CARD_ELEMENT_OPTIONS = {
  style: {
    base: {
      color: "gray",
      "::placeholder": {
        color: "gray"
      }
    }
  }
};

const stripePromise = loadStripe(process.env.REACT_APP_STRIPE_PUBLIC_KEY);

const CheckoutForm = ({ credits }) => {
  const classes = useStyles(useStyles);
  const stripe = useStripe();
  const elements = useElements();
  const isMobile = window.matchMedia("(max-width: 900px)").matches;

  const handlePurchaseCredits = async (event) => {
    event.preventDefault();

    if (!stripe || !elements) {
      return;
    }

    let cardElement = null;
    if (isMobile) {
      cardElement = elements.getElement(CardNumberElement);
    } else {
      cardElement = elements.getElement(CardElement);
    }
    const { error, paymentMethod } = await stripe.createPaymentMethod({
      type: "card",
      card: cardElement
    });

    if (error) {
      console.log("[error]", error);
    } else {
      _database
        .api("POST", "/profile/PurchaseCredits", {
          credits,
          paymentMethodId: paymentMethod.id,
          last4Digits: paymentMethod.card.last4
        })
        .then((result) => {
          if (result) {
            if (result.status === 1) {
              _notify.success("Purchase Complete!", 500, () => {
                window.location = "/";
              });
            } else {
              _notify.error(result.errorResult);
            }
          } else {
            _notify.error(
              "Could not get session. A unexpected error occurred, check your network connection."
            );
          }
        });
      console.log("[PaymentMethod]", paymentMethod);
    }
  };

  return (
    <form onSubmit={handlePurchaseCredits}>
      <Container className={classes.container}>
        <Paper className={classes.paper}>
          {isMobile ? (
            <>
              <Typography>Card info</Typography>
              <CardNumberElement
                options={CARD_ELEMENT_OPTIONS}
                className={classes.payment_form}
              />
              <CardExpiryElement
                options={CARD_ELEMENT_OPTIONS}
                className={classes.payment_form}
              />
              <CardCvcElement
                options={CARD_ELEMENT_OPTIONS}
                className={classes.payment_form}
              />
            </>
          ) : (
            <CardElement
              options={CARD_ELEMENT_OPTIONS}
              className={classes.payment_form}
            />
          )}
        </Paper>
        <Button
          className={classes.button}
          variant="contained"
          color="primary"
          type="submit"
          disabled={!stripe}
        >
          Pay
        </Button>
      </Container>
    </form>
  );
};

const CreditPayment = ({ handleClose, submit, submitSet }) => {
  const classes = useStyles(useStyles);
  const [credits, setCredits] = useState(
    localStorageService.getItem("creditCost") ?? "5"
  );

  return (
    <Container className={classes.container} maxWidth="xs">
      <h2>Purchase Credits</h2>
      <Elements stripe={stripePromise}>
        <FormControl>
          <InputLabel>Cost</InputLabel>
          <Select value={credits} onChange={(e) => setCredits(e.target.value)}>
            <MenuItem value="5">$4.99 - 5 Credits</MenuItem>
            <MenuItem value="10">$9.99 - 10 Credits</MenuItem>
            <MenuItem value="25">$24.99 - 25 Credits</MenuItem>
            <MenuItem value="100">$99.99 - 100 Credits</MenuItem>
          </Select>
        </FormControl>
        <CheckoutForm credits={credits} />
      </Elements>
    </Container>
  );
};

export default CreditPayment;
