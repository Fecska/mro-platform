import { lazy } from 'react';
import { Routes, Route, Navigate } from 'react-router-dom';
import { ProtectedRoute } from './ProtectedRoute';
import { AppLayout } from '@/components/layout/AppLayout';

// Lazy-loaded pages — each feature owns its page components
const LoginPage       = lazy(() => import('@/pages/auth/LoginPage'));
const DashboardPage   = lazy(() => import('@/pages/DashboardPage'));
const AircraftPage    = lazy(() => import('@/pages/aircraft/AircraftPage'));
const AircraftDetail  = lazy(() => import('@/pages/aircraft/AircraftDetailPage'));
const DefectsPage     = lazy(() => import('@/pages/defects/DefectsPage'));
const WorkOrdersPage  = lazy(() => import('@/pages/workorders/WorkOrdersPage'));
const InventoryPage   = lazy(() => import('@/pages/inventory/InventoryPage'));
const PersonnelPage   = lazy(() => import('@/pages/personnel/PersonnelPage'));
const NotFoundPage    = lazy(() => import('@/pages/NotFoundPage'));

export function AppRoutes() {
  return (
    <Routes>
      {/* Public */}
      <Route path="/login" element={<LoginPage />} />

      {/* Protected — requires authentication */}
      <Route element={<ProtectedRoute />}>
        <Route element={<AppLayout />}>
          <Route index element={<Navigate to="/dashboard" replace />} />
          <Route path="dashboard"          element={<DashboardPage />} />
          <Route path="aircraft"           element={<AircraftPage />} />
          <Route path="aircraft/:id"       element={<AircraftDetail />} />
          <Route path="defects"            element={<DefectsPage />} />
          <Route path="work-orders"        element={<WorkOrdersPage />} />
          <Route path="inventory"          element={<InventoryPage />} />
          <Route path="personnel"          element={<PersonnelPage />} />
        </Route>
      </Route>

      <Route path="*" element={<NotFoundPage />} />
    </Routes>
  );
}
