import Checkbox from '@mui/material/Checkbox'
import DeleteOutlineIcon from '@mui/icons-material/DeleteOutlined'
import IconButton from '@mui/material/IconButton'
import ListItem from '@mui/material/ListItem'
import ListItemButton from '@mui/material/ListItemButton'
import ListItemIcon from '@mui/material/ListItemIcon'
import ListItemText from '@mui/material/ListItemText'
import Tooltip from '@mui/material/Tooltip'
import { useDeleteTask, useUpdateTask, type Task } from '../api/tasks'
import { useNotifications } from '../notifications/useNotifications'

const dateFormat = new Intl.DateTimeFormat(undefined, { dateStyle: 'medium', timeStyle: 'short' })

export default function TaskListItem({ task }: { task: Task }) {
  const updateTask = useUpdateTask()
  const deleteTask = useDeleteTask()
  const { notifySuccess, notifyError } = useNotifications()
  const busy = updateTask.isPending || deleteTask.isPending

  function toggleCompletion() {
    const isCompleted = !task.isCompleted

    updateTask.mutate(
      { id: task.id, isCompleted },
      {
        onSuccess: () => notifySuccess(isCompleted ? 'Task completed.' : 'Task reopened.'),
        onError: (error) => notifyError(`Could not update the task: ${error.message}`),
      },
    )
  }

  function remove() {
    deleteTask.mutate(task.id, {
      onSuccess: () => notifySuccess('Task deleted.'),
      onError: (error) => notifyError(`Could not delete the task: ${error.message}`),
    })
  }

  return (
    <ListItem
      disablePadding
      secondaryAction={
        <Tooltip title="Delete task">
          <span>
            <IconButton
              edge="end"
              aria-label={`Delete ${task.title}`}
              disabled={busy}
              onClick={remove}
            >
              <DeleteOutlineIcon />
            </IconButton>
          </span>
        </Tooltip>
      }
    >
      <ListItemButton
        dense
        disabled={busy}
        onClick={toggleCompletion}
      >
        <ListItemIcon sx={{ minWidth: 42 }}>
          <Checkbox
            edge="start"
            tabIndex={-1}
            disableRipple
            checked={task.isCompleted}
            slotProps={{ input: { 'aria-label': task.title } }}
          />
        </ListItemIcon>
        <ListItemText
          primary={task.title}
          secondary={`Created ${dateFormat.format(new Date(task.createdAt))}`}
          slotProps={{
            primary: {
              sx: {
                textDecoration: task.isCompleted ? 'line-through' : 'none',
                color: task.isCompleted ? 'text.secondary' : 'text.primary',
              },
            },
          }}
        />
      </ListItemButton>
    </ListItem>
  )
}
