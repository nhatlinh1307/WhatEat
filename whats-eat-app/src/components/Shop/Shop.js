import React, { useState, useEffect } from "react";
import axios from "axios";
import ShopSidebar from "../ShopSidebar/ShopSidebar";
import CustomLineChart from "../Charts/CustomLineChart";
import CustomBarChart from "../Charts/CustomBarChart";
import CustomPieChart from "../Charts/CustomPieChart";
import SaleList from '../../components/SaleList/SaleList';
import "./Shop.css";

const Shop = ({ storeId }) => {
  const [dataIncomeByDay, setDataIncomeByDay] = useState([]);
  const [dataStatusOrders, setDataStatusOrders] = useState([]);
  const [dataBestProducts, setDataBestProducts] = useState([]);
  const [dataBestCategories, setDataBestCategories] = useState([]);
  const [month, setMonth] = useState();
  const [year, setYear] = useState();

  const newDataIncome = [];
  dataIncomeByDay.map((income) => {
    const { day, total } = income;
    newDataIncome.push({ Ngày: `Ngày ${day}`, "Doanh thu": total / 1000000 });
  });

  const newDataStatusOrders = [];
  dataStatusOrders.map((status) => {
    const { orderStatusName, count } = status;
    if (orderStatusName === "waiting") {
      newDataStatusOrders.push({
        name: `Chờ xác nhận`,
        value: count,
      });
    }
    if (orderStatusName === "delivering") {
      newDataStatusOrders.push({
        name: `Đang vận chuyển`,
        value: count,
      });
    }
    if (orderStatusName === "delivered") {
      newDataStatusOrders.push({
        name: `Đã giao hàng`,
        value: count,
      });
    }
    if (orderStatusName === "canceled") {
      newDataStatusOrders.push({
        name: `Đơn hủy`,
        value: count,
      });
    }
  });

  const newDataBestProducts = [];
  dataBestProducts.map((product) => {
    newDataBestProducts.push({
      "Mã sản phẩm": product.productId,
      "Số lượng bán": product.amount,
      "Tên sản phẩm": product.productName,
    });
  });
  console.log({ newDataBestProducts });

  const newDataBestCategories = [];
  dataBestCategories.map((category) => {
    newDataBestCategories.push({
      "Mã danh mục": category.productCategoryId,
      "Số lượng bán": category.amount,
      "Tên danh mục": category.productCategoryName,
    });
  });

  const [sales, setSales] = useState([])

  useEffect(() => {
    axios({//need data
      method: "GET",
      url: `${process.env.REACT_APP_PYTHON_API_KEY}/sale`,
      headers: {
        'Content-Type':'application/json'
      }
    })
    .then(resp => resp.json())
    .then(resp => setSales(resp))
    .catch(error => console.log(error))
  },[])

  useEffect(() => {
    axios({
      method: "GET",
      url: `${process.env.REACT_APP_ASP_API_KEY}/api/Store/income-by-day`,
    })
      .then((res) => {
        setDataIncomeByDay(res.data);
        setMonth(res.data[0].month);
        setYear(res.data[0].year);
      })
      .catch((err) => {
        console.log(err);
      });
  }, []);
  useEffect(() => {
    axios({
      method: "GET",
      url: `${process.env.REACT_APP_ASP_API_KEY}/api/Store/number-order-by-statuses`,
    })
      .then((res) => {
        setDataStatusOrders(res.data);
      })
      .catch((err) => {
        console.log(err);
      });
  }, []);
  useEffect(() => {
    axios({
      method: "GET",
      url: `${process.env.REACT_APP_ASP_API_KEY}/api/Store/best-seller-of-months`,
    })
      .then((res) => {
        setDataBestProducts(res.data);
      })
      .catch((err) => {
        console.log(err);
      });
  }, []);
  useEffect(() => {
    axios({
      method: "GET",
      url: `${process.env.REACT_APP_ASP_API_KEY}/api/Store/best-category-of-months`,
    })
      .then((res) => {
        setDataBestCategories(res.data);
      })
      .catch((err) => {
        console.log(err);
      });
  }, []);
  return (
    <div className="shop-cpn">
      <div className="shop-cpn-fluid">
        <div className="shop-cpn-container">
          <ShopSidebar storeId={storeId} />
          <div className="content-container">
            {newDataIncome.length > 0 && (
              <div className="monthly-income">
                <h1>
                  Doanh thu theo từng ngày trong tháng {month}/{year}
                </h1>
                <CustomLineChart data={newDataIncome} />
              </div>
            )}
            <div className="best-seller-cate">
              <h1>Số lượng sản phẩm đã bán được cao nhất trong tháng</h1>
              <SaleList sales = {sales} />
              <CustomBarChart data={newDataBestProducts} sales = {sales}/>
            </div>

            {newDataStatusOrders.length > 0 && (
              <div className="best-seller-product">
                <h1>Tỉ lệ đơn hàng</h1>
                <CustomPieChart data={newDataStatusOrders} />
              </div>
            )}
            {/*<div className="all-order-status">
              <h1>Doanh thu trong ngày</h1>
              <CustomLineChart data={mock_data_order_rate} />
            </div> */}
          </div>
        </div>
      </div>
    </div>
  );
};

export default Shop;