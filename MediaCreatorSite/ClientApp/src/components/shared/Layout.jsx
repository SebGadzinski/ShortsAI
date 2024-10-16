import React, { useContext } from "react";
import Header from "./Header";
import { makeStyles } from "@mui/styles";
import Footer from "./Footer";
import { Container } from "@mui/material";
import AuthModal from "../modals/AuthModal";
import { BaseModalContext } from "../providers/BaseModalProvider";
import BaseModal from "../modals/BaseModal";
import CreditPayment from "../pages/profile/components/CreditPayment";

const useStyles = makeStyles((theme) => ({
  content: {
    flexGrow: 1,
    padding: theme.spacing(3)
  },
  outerDiv: {
    width: "100%",
    margin: "20px auto !important"
  }
}));

const Layout = ({
  handleAuthModalOpen,
  showAuthModal,
  handleAuthModalClose,
  children,
  selectTheme,
  themes
}) => {
  const classes = useStyles();
  const { content } = useContext(BaseModalContext);

  return (
    <Container className={classes.outerDiv}>
      <Header
        handleAuthModalOpen={handleAuthModalOpen}
        themes={themes}
        selectTheme={selectTheme}
        logo="/images/logo.png"
      />
      <main className={classes.content}>{children}</main>
      <AuthModal open={showAuthModal} onClose={handleAuthModalClose} />
      <BaseModal>
        {content === "CreditPayment" ? <CreditPayment /> : <></>}
      </BaseModal>
      <Footer></Footer>
    </Container>
  );
};

export default Layout;
