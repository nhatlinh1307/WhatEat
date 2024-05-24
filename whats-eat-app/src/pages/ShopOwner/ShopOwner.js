import { Form, Input, message, Modal, Table } from "antd";
import "antd/dist/antd.css";
import React, { useEffect, useState } from "react";
import { useSelector } from "react-redux";
import axios from "axios";
import { useLocation } from "react-router-dom";
import Footer from "../../components/Footer/Footer";
import ShopSidebar from "../../components/ShopSidebar/ShopSidebar";
import ShopList from '../../components/ShopList/ShopList';
import "./ShopOwner.css";

const layout = {
  labelCol: { span: 8 },
  wrapperCol: { span: 16 },
};

/* eslint-disable no-template-curly-in-string */
const validateMessages = {
  required: "Vui lòng nhập ${label}!",
  types: {
    email: "${label} không hợp lệ!",
    number: "${label} không hợp lệ!",
  },
};
const ShopOwner = () => {
  const token = useSelector((state) => state.auth.userInfo.token);
  const [users, setUsers] = useState([])
  const parseJwt = (token) => {
    try {
      return JSON.parse(atob(token.split(".")[1]));
    } catch (e) {
      return null;
    }
  };
  let Id = null;
  const parsedToken = parseJwt(token);
  if (parsedToken !== null && parsedToken.hasOwnProperty('Id')) {
    Id = parsedToken.Id;
  }
  useEffect(() => {
    axios({//need data
      method: "GET",
      url: `${process.env.REACT_APP_PYTHON_API_KEY}/getshopuser`,
      headers: {
        'Content-Type':'application/json',
        'Authorization':'Bearer '+token },
      params: {
        Id: Id
      }
    })
    .then(resp =>setUsers(resp.data))
    .catch(error => console.log(error.response));
  },[Id])
  const onFormFinish = (values) => {
    axios({
      method: "PUT",
      url: `${process.env.REACT_APP_ASP_API_KEY}/api/Store/info`,
      headers: { Authorization: `Bearer ${token}` },
      data: values,
    })
      .then((res) => {
        message.success("Thay đổi thông tin thành công!");
      })
      .catch((err) => {
        console.log(err);
        message.error("Thay đổi thông tin thất bại!");
      });
  };

  return (
    <div className="shop-profile">
      <div className="shop-profile-fluid">
        <div className="shop-profile-container">
          <ShopSidebar />
          <div className="content-container">
            <h1 className="title">Danh sách Shop</h1>
            <p className="note">
              Xem danh sách Shop và chủ của shop
            </p>
            <div className = "App" >
              <tr>
                <td><b>Tên cửa hàng</b></td>
                <td><b>Email</b></td>
              </tr>
            {users.map (order => {
                      return (

                        <div key = {order.StoreId}>
                            <tr>
                              <td>{order.ShopName}</td>
                              <td>{order.Email}</td>
                            </tr>

                        </div>
                      )
                    })}

            </div>
          </div>
        </div>
      </div>
      <Footer />
    </div>
  );
};

export default ShopOwner;
